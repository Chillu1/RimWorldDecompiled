using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class HistoryEventsManager : IExposable
{
	private struct HistoryEventRecords : IExposable
	{
		public List<int> ticksGame;

		public List<int> customGoodwill;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref ticksGame, "ticksGame", LookMode.Value);
			Scribe_Collections.Look(ref customGoodwill, "customGoodwill", LookMode.Value);
		}
	}

	private DefMap<HistoryEventDef, HistoryEventRecords> colonistEvents = new DefMap<HistoryEventDef, HistoryEventRecords>();

	private Dictionary<Faction, DefMap<HistoryEventDef, HistoryEventRecords>> eventsAffectingFaction = new Dictionary<Faction, DefMap<HistoryEventDef, HistoryEventRecords>>();

	private List<Faction> tmpFactions;

	private List<DefMap<HistoryEventDef, HistoryEventRecords>> tmpDefMaps;

	private const int CheckRemoveOldEventsTicksInterval = 10000;

	private const int MinCountToRemoveOld = 5;

	private const int MaxCountToKeep = 20;

	private const float RemoveOldScoreThreshold = 0.5f;

	private static readonly SimpleCurve DaysSinceEventToScoreFactor = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 0.1f),
		new CurvePoint(300f, 0f)
	};

	private static readonly SimpleCurve GoodwillImpactToScoreFactor = new SimpleCurve
	{
		new CurvePoint(0f, 0.55f),
		new CurvePoint(20f, 1f),
		new CurvePoint(100f, 10f),
		new CurvePoint(200f, 15f)
	};

	private static List<(HistoryEventDef, int, float)> tmpEvents = new List<(HistoryEventDef, int, float)>();

	public void RecordEvent(HistoryEvent historyEvent, bool canApplySelfTookThoughts = true)
	{
		try
		{
			IdeoUtility.Notify_HistoryEvent(historyEvent, canApplySelfTookThoughts);
		}
		catch (Exception ex)
		{
			Log.Error("Error while notifying ideos of a HistoryEvent: " + ex);
		}
		if (!historyEvent.args.TryGetArg(HistoryEventArgsNames.CustomGoodwill, out int arg))
		{
			arg = 0;
		}
		if (historyEvent.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn arg2) && arg2.IsColonist)
		{
			HistoryEventRecords value = colonistEvents[historyEvent.def];
			if (value.ticksGame == null)
			{
				value.ticksGame = new List<int>();
				value.customGoodwill = new List<int>();
				colonistEvents[historyEvent.def] = value;
			}
			value.ticksGame.Add(Find.TickManager.TicksGame);
			value.customGoodwill.Add(arg);
			if (value.ticksGame.Count > historyEvent.def.maxRemembered)
			{
				value.ticksGame.RemoveRange(0, value.ticksGame.Count - historyEvent.def.maxRemembered);
				value.customGoodwill.RemoveRange(0, value.ticksGame.Count - historyEvent.def.maxRemembered);
			}
		}
		if (historyEvent.args.TryGetArg(HistoryEventArgsNames.AffectedFaction, out Faction arg3))
		{
			if (!eventsAffectingFaction.TryGetValue(arg3, out var value2))
			{
				value2 = new DefMap<HistoryEventDef, HistoryEventRecords>();
				eventsAffectingFaction.Add(arg3, value2);
			}
			HistoryEventRecords value3 = value2[historyEvent.def];
			if (value3.ticksGame == null)
			{
				value3.ticksGame = new List<int>();
				value3.customGoodwill = new List<int>();
				value2[historyEvent.def] = value3;
			}
			value3.ticksGame.Add(Find.TickManager.TicksGame);
			value3.customGoodwill.Add(arg);
			if (value3.ticksGame.Count > historyEvent.def.maxRemembered)
			{
				value3.ticksGame.RemoveRange(0, value3.ticksGame.Count - historyEvent.def.maxRemembered);
				value3.customGoodwill.RemoveRange(0, value3.ticksGame.Count - historyEvent.def.maxRemembered);
			}
		}
	}

	public void HistoryEventsManagerTick()
	{
		if (Find.TickManager.TicksGame % 10000 != 0)
		{
			return;
		}
		foreach (KeyValuePair<Faction, DefMap<HistoryEventDef, HistoryEventRecords>> item in eventsAffectingFaction)
		{
			CheckRemoveOldEvents(item.Value);
		}
	}

	private void CheckRemoveOldEvents(DefMap<HistoryEventDef, HistoryEventRecords> ev)
	{
		tmpEvents.Clear();
		List<HistoryEventDef> allDefsListForReading = DefDatabase<HistoryEventDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			HistoryEventRecords historyEventRecords = ev[allDefsListForReading[i]];
			if (historyEventRecords.ticksGame != null)
			{
				for (int j = 0; j < historyEventRecords.ticksGame.Count; j++)
				{
					float score = GetScore(historyEventRecords.ticksGame[j], historyEventRecords.customGoodwill[j]);
					tmpEvents.Add((allDefsListForReading[i], j, score));
				}
			}
		}
		if (tmpEvents.Count <= 5)
		{
			return;
		}
		if (tmpEvents.Count > 20)
		{
			tmpEvents.SortByDescending(((HistoryEventDef, int, float) x) => x.Item3);
			for (int num = 20; num < tmpEvents.Count; num++)
			{
				Remove(tmpEvents[num].Item1, tmpEvents[num].Item2);
			}
			tmpEvents.RemoveRange(20, tmpEvents.Count - 20);
		}
		int num2 = tmpEvents.Count - 1;
		while (num2 >= 0 && tmpEvents.Count > 5)
		{
			if (tmpEvents[num2].Item3 < 0.5f)
			{
				Remove(tmpEvents[num2].Item1, tmpEvents[num2].Item2);
				tmpEvents.RemoveAt(num2);
			}
			num2--;
		}
		for (int num3 = 0; num3 < allDefsListForReading.Count; num3++)
		{
			if (ev[allDefsListForReading[num3]].ticksGame != null)
			{
				ev[allDefsListForReading[num3]].ticksGame.RemoveAll((int x) => x == int.MinValue);
				ev[allDefsListForReading[num3]].customGoodwill.RemoveAll((int x) => x == int.MinValue);
			}
		}
		void Remove(HistoryEventDef def, int index)
		{
			ev[def].ticksGame[index] = int.MinValue;
			ev[def].customGoodwill[index] = int.MinValue;
		}
	}

	private float GetScore(int ticksOccurred, int goodwill)
	{
		float x = (Find.TickManager.TicksGame - ticksOccurred).TicksToDays();
		return DaysSinceEventToScoreFactor.Evaluate(x) * GoodwillImpactToScoreFactor.Evaluate(goodwill);
	}

	public bool Any(HistoryEventDef def, Faction forFaction = null)
	{
		if (forFaction != null)
		{
			if (eventsAffectingFaction.TryGetValue(forFaction, out var value))
			{
				return !value[def].ticksGame.NullOrEmpty();
			}
			return false;
		}
		return !colonistEvents[def].ticksGame.NullOrEmpty();
	}

	public int GetLastTicksGame(HistoryEventDef def, Faction forFaction = null)
	{
		if (!Any(def, forFaction))
		{
			return -999999;
		}
		if (forFaction != null)
		{
			if (eventsAffectingFaction.TryGetValue(forFaction, out var value))
			{
				return value[def].ticksGame[value[def].ticksGame.Count - 1];
			}
			return -999999;
		}
		return colonistEvents[def].ticksGame[colonistEvents[def].ticksGame.Count - 1];
	}

	public int GetRecentCountWithinTicks(HistoryEventDef def, int duration, Faction forFaction = null)
	{
		if (!Any(def, forFaction))
		{
			return 0;
		}
		if (forFaction != null)
		{
			if (eventsAffectingFaction.TryGetValue(forFaction, out var value))
			{
				return GenCollection.GetCountGreaterOrEqualInSortedList(value[def].ticksGame, Find.TickManager.TicksGame - duration);
			}
			return 0;
		}
		return GenCollection.GetCountGreaterOrEqualInSortedList(colonistEvents[def].ticksGame, Find.TickManager.TicksGame - duration);
	}

	public void GetRecent(HistoryEventDef def, int duration, List<int> outTicks, List<int> outCustomGoodwill = null, Faction forFaction = null)
	{
		outTicks.Clear();
		outCustomGoodwill?.Clear();
		if (!Any(def, forFaction))
		{
			return;
		}
		if (forFaction != null)
		{
			if (eventsAffectingFaction.TryGetValue(forFaction, out var value))
			{
				int num = value[def].ticksGame.Count - 1;
				while (num >= 0 && value[def].ticksGame[num] >= Find.TickManager.TicksGame - duration)
				{
					outTicks.Add(value[def].ticksGame[num]);
					outCustomGoodwill?.Add(value[def].customGoodwill[num]);
					num--;
				}
			}
		}
		else
		{
			int num2 = colonistEvents[def].ticksGame.Count - 1;
			while (num2 >= 0 && colonistEvents[def].ticksGame[num2] >= Find.TickManager.TicksGame - duration)
			{
				outTicks.Add(colonistEvents[def].ticksGame[num2]);
				outCustomGoodwill?.Add(colonistEvents[def].customGoodwill[num2]);
				num2--;
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref colonistEvents, "colonistEvents");
		Scribe_Collections.Look(ref eventsAffectingFaction, "eventsAffectingFaction", LookMode.Reference, LookMode.Deep, ref tmpFactions, ref tmpDefMaps);
	}
}
