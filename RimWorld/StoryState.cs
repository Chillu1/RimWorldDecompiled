using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StoryState : IExposable
	{
		private IIncidentTarget target;

		private int lastThreatBigTick = -1;

		private Dictionary<int, int> colonistCountTicks = new Dictionary<int, int>();

		public Dictionary<IncidentDef, int> lastFireTicks = new Dictionary<IncidentDef, int>();

		private List<QuestScriptDef> recentRandomQuests = new List<QuestScriptDef>();

		private List<QuestScriptDef> recentRandomDecrees = new List<QuestScriptDef>();

		private int lastRoyalFavorQuestTick = -1;

		private const int RecentRandomQuestsMaxStorage = 5;

		public IIncidentTarget Target => target;

		public List<QuestScriptDef> RecentRandomQuests => recentRandomQuests;

		public List<QuestScriptDef> RecentRandomDecrees => recentRandomDecrees;

		public int LastRoyalFavorQuestTick => lastRoyalFavorQuestTick;

		public int LastThreatBigTick
		{
			get
			{
				if (lastThreatBigTick > Find.TickManager.TicksGame + 1000)
				{
					Log.Error("Latest big threat queue time was " + lastThreatBigTick + " at tick " + Find.TickManager.TicksGame + ". This is too far in the future. Resetting.");
					lastThreatBigTick = Find.TickManager.TicksGame - 1;
				}
				return lastThreatBigTick;
			}
		}

		public StoryState(IIncidentTarget target)
		{
			this.target = target;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastThreatBigTick, "lastThreatBigTick", 0, forceSave: true);
			Scribe_Collections.Look(ref lastFireTicks, "lastFireTicks", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref recentRandomQuests, "recentRandomQuests", LookMode.Def);
			Scribe_Collections.Look(ref recentRandomDecrees, "recentRandomDecrees", LookMode.Def);
			Scribe_Collections.Look(ref colonistCountTicks, "colonistCountTicks", LookMode.Value, LookMode.Value);
			Scribe_Values.Look(ref lastRoyalFavorQuestTick, "lastRoyalFavorQuestTick", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (recentRandomQuests == null)
				{
					recentRandomQuests = new List<QuestScriptDef>();
				}
				if (recentRandomDecrees == null)
				{
					recentRandomDecrees = new List<QuestScriptDef>();
				}
				if (colonistCountTicks == null)
				{
					colonistCountTicks = new Dictionary<int, int>();
				}
				RecordPopulationIncrease();
			}
		}

		public void Notify_IncidentFired(FiringIncident fi)
		{
			if (!fi.parms.forced && fi.parms.target == target)
			{
				int ticksGame = Find.TickManager.TicksGame;
				if (fi.def.category == IncidentCategoryDefOf.ThreatBig)
				{
					lastThreatBigTick = ticksGame;
					Find.StoryWatcher.statsRecord.numThreatBigs++;
				}
				if (lastFireTicks.ContainsKey(fi.def))
				{
					lastFireTicks[fi.def] = ticksGame;
				}
				else
				{
					lastFireTicks.Add(fi.def, ticksGame);
				}
				if (fi.def == IncidentDefOf.GiveQuest_Random)
				{
					RecordRandomQuestFired(fi.parms.questScriptDef);
				}
			}
		}

		public void RecordRandomQuestFired(QuestScriptDef questScript)
		{
			recentRandomQuests.Insert(0, questScript);
			while (recentRandomQuests.Count > 5)
			{
				recentRandomQuests.RemoveAt(recentRandomQuests.Count - 1);
			}
			if (questScript.canGiveRoyalFavor)
			{
				lastRoyalFavorQuestTick = Find.TickManager.TicksGame;
			}
		}

		public void RecordDecreeFired(QuestScriptDef questScript)
		{
			recentRandomDecrees.Insert(0, questScript);
			while (recentRandomDecrees.Count > 5)
			{
				recentRandomDecrees.RemoveAt(recentRandomDecrees.Count - 1);
			}
		}

		public void RecordPopulationIncrease()
		{
			int count = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Count;
			if (!colonistCountTicks.ContainsKey(count))
			{
				colonistCountTicks.Add(count, Find.TickManager.TicksGame);
			}
		}

		public int GetTicksFromColonistCount(int count)
		{
			if (!colonistCountTicks.ContainsKey(count))
			{
				colonistCountTicks.Add(count, Find.TickManager.TicksGame);
			}
			return colonistCountTicks[count];
		}

		public void CopyTo(StoryState other)
		{
			other.lastThreatBigTick = lastThreatBigTick;
			other.lastFireTicks.Clear();
			foreach (KeyValuePair<IncidentDef, int> lastFireTick in lastFireTicks)
			{
				other.lastFireTicks.Add(lastFireTick.Key, lastFireTick.Value);
			}
			other.RecentRandomQuests.Clear();
			other.recentRandomQuests.AddRange(RecentRandomQuests);
			other.RecentRandomDecrees.Clear();
			other.RecentRandomDecrees.AddRange(RecentRandomDecrees);
			other.lastRoyalFavorQuestTick = lastRoyalFavorQuestTick;
			other.colonistCountTicks.Clear();
			foreach (KeyValuePair<int, int> colonistCountTick in colonistCountTicks)
			{
				other.colonistCountTicks.Add(colonistCountTick.Key, colonistCountTick.Value);
			}
		}
	}
}
