using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class BattleLog : IExposable
	{
		private List<Battle> battles = new List<Battle>();

		private const int BattleHistoryLength = 20;

		private HashSet<LogEntry> activeEntries;

		public List<Battle> Battles => battles;

		public void Add(LogEntry entry)
		{
			Battle battle = null;
			foreach (Pawn concern in entry.GetConcerns())
			{
				Battle battleActive = concern.records.BattleActive;
				if (battle == null)
				{
					battle = battleActive;
				}
				else if (battleActive != null)
				{
					battle = ((battle.Importance > battleActive.Importance) ? battle : battleActive);
				}
			}
			if (battle == null)
			{
				battle = Battle.Create();
				battles.Insert(0, battle);
			}
			foreach (Pawn concern2 in entry.GetConcerns())
			{
				Battle battleActive2 = concern2.records.BattleActive;
				if (battleActive2 != null && battleActive2 != battle)
				{
					battle.Absorb(battleActive2);
					battles.Remove(battleActive2);
				}
				concern2.records.EnterBattle(battle);
			}
			battle.Add(entry);
			activeEntries = null;
			ReduceToCapacity();
		}

		private void ReduceToCapacity()
		{
			int num = battles.Count((Battle btl) => btl.AbsorbedBy == null);
			while (num > 20 && battles[battles.Count - 1].LastEntryTimestamp + Mathf.Max(420000, 5000) < Find.TickManager.TicksGame)
			{
				if (battles[battles.Count - 1].AbsorbedBy == null)
				{
					num--;
				}
				battles.RemoveAt(battles.Count - 1);
				activeEntries = null;
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref battles, "battles", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && battles == null)
			{
				battles = new List<Battle>();
			}
		}

		public bool AnyEntryConcerns(Pawn p)
		{
			for (int i = 0; i < battles.Count; i++)
			{
				if (battles[i].Concerns(p))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsEntryActive(LogEntry log)
		{
			if (activeEntries == null)
			{
				activeEntries = new HashSet<LogEntry>();
				for (int i = 0; i < battles.Count; i++)
				{
					List<LogEntry> entries = battles[i].Entries;
					for (int j = 0; j < entries.Count; j++)
					{
						activeEntries.Add(entries[j]);
					}
				}
			}
			return activeEntries.Contains(log);
		}

		public void RemoveEntry(LogEntry log)
		{
			for (int i = 0; i < battles.Count && !battles[i].Entries.Remove(log); i++)
			{
			}
		}

		public void Notify_PawnDiscarded(Pawn p, bool silentlyRemoveReferences)
		{
			for (int num = battles.Count - 1; num >= 0; num--)
			{
				battles[num].Notify_PawnDiscarded(p, silentlyRemoveReferences);
			}
		}
	}
}
