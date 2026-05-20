using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Grammar;

namespace Verse;

public class Battle : IExposable, ILoadReferenceable
{
	public const int TicksForBattleExit = 5000;

	private List<LogEntry> entries = new List<LogEntry>();

	private string battleName;

	private Battle absorbedBy;

	private HashSet<Pawn> concerns = new HashSet<Pawn>();

	private int loadID;

	private int creationTimestamp;

	public int Importance => entries.Count;

	public int CreationTimestamp => creationTimestamp;

	public int LastEntryTimestamp
	{
		get
		{
			if (entries.Count <= 0)
			{
				return 0;
			}
			return entries[entries.Count - 1].Timestamp;
		}
	}

	public Battle AbsorbedBy => absorbedBy;

	public List<LogEntry> Entries => entries;

	public static Battle Create()
	{
		return new Battle
		{
			loadID = Find.UniqueIDsManager.GetNextBattleID(),
			creationTimestamp = Find.TickManager.TicksGame
		};
	}

	public string GetName()
	{
		if (battleName.NullOrEmpty())
		{
			HashSet<Faction> hashSet = new HashSet<Faction>(concerns.Select((Pawn p) => p.Faction));
			GrammarRequest request = default(GrammarRequest);
			if (concerns.Count == 1 && hashSet.Count((Faction f) => f != null) < 2)
			{
				request.Includes.Add(RulePackDefOf.Battle_Solo);
				request.Rules.AddRange(GrammarUtility.RulesForPawn("PARTICIPANT1", concerns.First()));
			}
			else if (concerns.Count == 2)
			{
				request.Includes.Add(RulePackDefOf.Battle_Duel);
				request.Rules.AddRange(GrammarUtility.RulesForPawn("PARTICIPANT1", concerns.First()));
				request.Rules.AddRange(GrammarUtility.RulesForPawn("PARTICIPANT2", concerns.Last()));
			}
			else if (hashSet.Count == 1)
			{
				request.Includes.Add(RulePackDefOf.Battle_Internal);
				request.Rules.AddRange(GrammarUtility.RulesForFaction("FACTION1", hashSet.First(), request.Constants));
			}
			else if (hashSet.Count == 2)
			{
				request.Includes.Add(RulePackDefOf.Battle_War);
				request.Rules.AddRange(GrammarUtility.RulesForFaction("FACTION1", hashSet.First(), request.Constants));
				request.Rules.AddRange(GrammarUtility.RulesForFaction("FACTION2", hashSet.Last(), request.Constants));
			}
			else
			{
				request.Includes.Add(RulePackDefOf.Battle_Brawl);
			}
			battleName = GrammarResolver.Resolve("r_battlename", request);
		}
		return battleName;
	}

	public void Add(LogEntry entry)
	{
		entries.Insert(0, entry);
		foreach (Thing concern in entry.GetConcerns())
		{
			if (concern is Pawn)
			{
				concerns.Add(concern as Pawn);
			}
		}
		battleName = null;
	}

	public void Absorb(Battle battle)
	{
		creationTimestamp = Mathf.Min(creationTimestamp, battle.creationTimestamp);
		entries.AddRange(battle.entries);
		concerns.AddRange(battle.concerns);
		entries = entries.OrderBy((LogEntry e) => e.Age).ToList();
		battle.entries.Clear();
		battle.concerns.Clear();
		battle.absorbedBy = this;
		battleName = null;
	}

	public bool Concerns(Pawn pawn)
	{
		return concerns.Contains(pawn);
	}

	public void Notify_PawnDiscarded(Pawn p, bool silentlyRemoveReferences)
	{
		if (!concerns.Contains(p))
		{
			return;
		}
		for (int num = entries.Count - 1; num >= 0; num--)
		{
			if (entries[num].Concerns(p))
			{
				if (!silentlyRemoveReferences)
				{
					Log.Warning("Discarding pawn " + p?.ToString() + ", but he is referenced by a battle log entry " + entries[num]?.ToString() + ".");
				}
				entries.RemoveAt(num);
			}
		}
		concerns.Remove(p);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref creationTimestamp, "creationTimestamp", 0);
		Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);
		Scribe_References.Look(ref absorbedBy, "absorbedBy");
		Scribe_Values.Look(ref battleName, "battleName");
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		concerns.Clear();
		foreach (Pawn item in entries.SelectMany((LogEntry e) => e.GetConcerns()).OfType<Pawn>())
		{
			concerns.Add(item);
		}
	}

	public string GetUniqueLoadID()
	{
		return "Battle_" + loadID;
	}
}
