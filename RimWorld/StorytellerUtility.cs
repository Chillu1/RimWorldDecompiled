using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class StorytellerUtility
{
	public const float GlobalPointsMinRangeFloor = 35f;

	public const float GlobalPointsMax = 10000f;

	public const float BuildingWealthFactor = 0.5f;

	private static readonly SimpleCurve PointsPerWealthCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(14000f, 0f),
		new CurvePoint(400000f, 2400f),
		new CurvePoint(700000f, 3600f),
		new CurvePoint(1000000f, 4200f)
	};

	public const float FixedWeathModeMaxThreatLevelInYears = 12f;

	public static readonly SimpleCurve FixedWealthModeMapWealthFromTimeCurve = new SimpleCurve
	{
		new CurvePoint(0f, 10000f),
		new CurvePoint(180f, 180000f),
		new CurvePoint(720f, 1000000f),
		new CurvePoint(1800f, 2500000f)
	};

	private const float PointsPerTameNonDownedCombatTrainableAnimalCombatPower = 0.08f;

	private const float PointsPerPlayerPawnFactorInContainer = 0.3f;

	private const float PointsPerPlayerPawnHealthSummaryLerpAmount = 0.65f;

	private static readonly SimpleCurve PointsPerColonistByWealthCurve = new SimpleCurve
	{
		new CurvePoint(0f, 15f),
		new CurvePoint(10000f, 15f),
		new CurvePoint(400000f, 140f),
		new CurvePoint(1000000f, 200f)
	};

	private static readonly SimpleCurve PointsFactorForPawnAgeYearsCurve = new SimpleCurve
	{
		new CurvePoint(3f, 0f),
		new CurvePoint(13f, 0.5f),
		new CurvePoint(18f, 1f)
	};

	private static readonly SimpleCurve PointsFactorForColonyMechsCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.2f),
		new CurvePoint(10000f, 0.2f),
		new CurvePoint(400000f, 0.3f),
		new CurvePoint(1000000f, 0.4f)
	};

	private static readonly SimpleCurve PointsFactorForColonySubhumanCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.2f),
		new CurvePoint(10000f, 0.2f),
		new CurvePoint(400000f, 0.3f),
		new CurvePoint(1000000f, 0.4f)
	};

	private const float PointsPerPlayerSlaveFactor = 0.75f;

	public const float CaravanWealthPointsFactor = 0.7f;

	public const float CaravanAnimalPointsFactor = 0.7f;

	public const float GravshipWealthPointsFactor = 0.7f;

	public static readonly FloatRange CaravanPointsRandomFactorRange = new FloatRange(0.7f, 0.9f);

	public static readonly FloatRange GravshipPointsRandomFactorRange = new FloatRange(0.7f, 0.9f);

	private static readonly SimpleCurve AllyIncidentFractionFromAllyFraction = new SimpleCurve
	{
		new CurvePoint(1f, 1f),
		new CurvePoint(0.25f, 0.6f)
	};

	public const float ProgressScorePerWealth = 0.0001f;

	public const float ProgressScorePerFreeColonist = 1f;

	private static Dictionary<IIncidentTarget, StoryState> tmpOldStoryStates = new Dictionary<IIncidentTarget, StoryState>();

	public static float GlobalPointsMin()
	{
		return Rand.RangeSeeded(35f, Find.Storyteller.difficulty.MinThreatPointsCeiling, Find.TickManager.TicksGame / 2500);
	}

	public static IncidentParms DefaultParmsNow(IncidentCategoryDef incCat, IIncidentTarget target)
	{
		if (incCat == null)
		{
			Log.Warning("Trying to get default parms for null incident category.");
		}
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = target;
		if (incCat.needsParmsPoints)
		{
			incidentParms.points = DefaultThreatPointsNow(target);
		}
		return incidentParms;
	}

	public static float GetProgressScore(IIncidentTarget target)
	{
		int num = 0;
		foreach (Pawn item in target.PlayerPawnsForStoryteller)
		{
			if (!item.IsQuestLodger() && item.IsFreeColonist)
			{
				num++;
			}
		}
		return (float)num * 1f + target.PlayerWealthForStoryteller * 0.0001f;
	}

	public static float DefaultThreatPointsNow(IIncidentTarget target)
	{
		if (target is Map { IsPocketMap: not false } map)
		{
			target = map.PocketMapParent.sourceMap;
		}
		float playerWealthForStoryteller = target.PlayerWealthForStoryteller;
		float num = PointsPerWealthCurve.Evaluate(playerWealthForStoryteller);
		float num2 = 0f;
		foreach (Pawn item in target.PlayerPawnsForStoryteller)
		{
			if (item.IsQuestLodger())
			{
				continue;
			}
			float num3 = 0f;
			if (item.IsFreeColonist)
			{
				num3 = PointsPerColonistByWealthCurve.Evaluate(playerWealthForStoryteller);
			}
			else if (item.IsAnimal && item.Faction == Faction.OfPlayer && !item.Downed && item.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
			{
				num3 = 0.08f * item.kindDef.combatPower;
				if (target is Caravan)
				{
					num3 *= 0.7f;
				}
			}
			else if (item.IsColonyMech && !item.Downed)
			{
				num3 = item.kindDef.combatPower * PointsFactorForColonyMechsCurve.Evaluate(playerWealthForStoryteller);
			}
			else if (item.IsSubhuman)
			{
				num3 = item.kindDef.combatPower * PointsFactorForColonySubhumanCurve.Evaluate(playerWealthForStoryteller);
			}
			if (num3 > 0f)
			{
				if (item.ParentHolder != null && item.ParentHolder is Building_CryptosleepCasket)
				{
					num3 *= 0.3f;
				}
				num3 = Mathf.Lerp(num3, num3 * item.health.summaryHealth.SummaryHealthPercent, 0.65f);
				if (item.IsSlaveOfColony)
				{
					num3 *= 0.75f;
				}
				if (ModsConfig.BiotechActive && item.RaceProps.Humanlike)
				{
					num3 *= PointsFactorForPawnAgeYearsCurve.Evaluate(item.ageTracker.AgeBiologicalYearsFloat);
				}
				num2 += num3;
			}
		}
		float num4 = (num + num2) * target.IncidentPointsRandomFactorRange.RandomInRange;
		float totalThreatPointsFactor = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
		float num5 = Mathf.Lerp(1f, totalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor);
		return Mathf.Clamp(num4 * num5 * Find.Storyteller.difficulty.threatScale * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate(GenDate.DaysPassedSinceSettle), GlobalPointsMin(), 10000f);
	}

	public static float DefaultSiteThreatPointsNow()
	{
		return SiteTuning.ThreatPointsToSiteThreatPointsCurve.Evaluate(DefaultThreatPointsNow(Find.World)) * SiteTuning.SitePointRandomFactorRange.RandomInRange;
	}

	public static float AllyIncidentFraction(bool fullAlliesOnly)
	{
		List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < allFactionsListForReading.Count; i++)
		{
			if (!allFactionsListForReading[i].Hidden && !allFactionsListForReading[i].IsPlayer && !allFactionsListForReading[i].temporary)
			{
				if (allFactionsListForReading[i].def.CanEverBeNonHostile)
				{
					num2++;
				}
				if (allFactionsListForReading[i].PlayerRelationKind == FactionRelationKind.Ally || (!fullAlliesOnly && !allFactionsListForReading[i].HostileTo(Faction.OfPlayer)))
				{
					num++;
				}
			}
		}
		if (num == 0)
		{
			return -1f;
		}
		float x = (float)num / Mathf.Max(num2, 1f);
		return AllyIncidentFractionFromAllyFraction.Evaluate(x);
	}

	public static void ShowFutureIncidentsDebugLogFloatMenu(bool currentMapOnly)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.Add(new FloatMenuOption("-All comps-", delegate
		{
			DebugLogTestFutureIncidents(currentMapOnly);
		}));
		List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
		for (int num = 0; num < storytellerComps.Count; num++)
		{
			StorytellerComp comp = storytellerComps[num];
			string text = comp.ToString();
			if (!text.NullOrEmpty())
			{
				list.Add(new FloatMenuOption(text, delegate
				{
					DebugLogTestFutureIncidents(currentMapOnly, comp);
				}));
			}
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public static void DebugLogTestFutureIncidents(bool currentMapOnly, StorytellerComp onlyThisComp = null, QuestPart onlyThisQuestPart = null, int numTestDays = 100)
	{
		StringBuilder stringBuilder = new StringBuilder();
		DebugGetFutureIncidents(numTestDays, currentMapOnly, out var incCountsForTarget, out var incCountsForComp, out var allIncidents, out var threatBigCount, stringBuilder, onlyThisComp, null, onlyThisQuestPart);
		new StringBuilder();
		string text = "Test future incidents for " + Find.Storyteller.def;
		if (onlyThisComp != null)
		{
			text = text + " (" + onlyThisComp?.ToString() + ")";
		}
		text = text + " (" + Find.TickManager.TicksGame.TicksToDays().ToString("F1") + "d - " + (Find.TickManager.TicksGame + numTestDays * 60000).TicksToDays().ToString("F1") + "d)";
		DebugLogIncidentsInternal(allIncidents, threatBigCount, incCountsForTarget, incCountsForComp, numTestDays, stringBuilder.ToString(), text);
	}

	public static void DebugLogTestFutureIncidents(ThreatsGeneratorParams parms)
	{
		StringBuilder stringBuilder = new StringBuilder();
		DebugGetFutureIncidents(20, currentMapOnly: true, out var incCountsForTarget, out var incCountsForComp, out var allIncidents, out var threatBigCount, stringBuilder, null, parms);
		new StringBuilder();
		string header = "Test future incidents for ThreatsGenerator " + parms?.ToString() + " (" + 20 + " days, difficulty " + Find.Storyteller.difficultyDef?.ToString() + ")";
		DebugLogIncidentsInternal(allIncidents, threatBigCount, incCountsForTarget, incCountsForComp, 20, stringBuilder.ToString(), header);
	}

	private static void DebugLogIncidentsInternal(List<Pair<IncidentDef, IncidentParms>> allIncidents, int threatBigCount, Dictionary<IIncidentTarget, int> incCountsForTarget, int[] incCountsForComp, int numTestDays, string incidentList, string header)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(header);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Points guess:            " + DefaultThreatPointsNow(Find.AnyPlayerHomeMap));
		stringBuilder.AppendLine("Incident count:          " + incCountsForTarget.Sum((KeyValuePair<IIncidentTarget, int> x) => x.Value));
		stringBuilder.AppendLine("Incident count per day:  " + ((float)incCountsForTarget.Sum((KeyValuePair<IIncidentTarget, int> x) => x.Value) / (float)numTestDays).ToString("F2"));
		stringBuilder.AppendLine("ThreatBig count:         " + threatBigCount);
		stringBuilder.AppendLine("ThreatBig count per day: " + ((float)threatBigCount / (float)numTestDays).ToString("F2"));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Incident count per def:");
		foreach (IncidentDef inc in from x in allIncidents.Select((Pair<IncidentDef, IncidentParms> x) => x.First).Distinct()
			orderby x.category.defName, x.defName
			select x)
		{
			int num = allIncidents.Where((Pair<IncidentDef, IncidentParms> i) => i.First == inc).Count();
			stringBuilder.AppendLine("  " + inc.category.defName.PadRight(20) + " " + inc.defName.PadRight(35) + " " + num);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Incident count per target:");
		foreach (KeyValuePair<IIncidentTarget, int> item in incCountsForTarget.OrderBy((KeyValuePair<IIncidentTarget, int> kvp) => kvp.Value))
		{
			stringBuilder.AppendLine("  " + item.Key.ToString().PadRight(30) + " " + item.Value);
		}
		if (ModsConfig.AnomalyActive)
		{
			List<IncidentCategoryDef> allDefsListForReading = DefDatabase<IncidentCategoryDef>.AllDefsListForReading;
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Anomaly % by category:");
			foreach (IncidentCategoryDef cat in allDefsListForReading)
			{
				if (Storyteller.AnomalyIncidents.Count((IncidentDef incident) => incident.category == cat) <= 0)
				{
					continue;
				}
				IEnumerable<Pair<IncidentDef, IncidentParms>> enumerable = allIncidents.Where((Pair<IncidentDef, IncidentParms> i) => i.First.category == cat);
				if (!enumerable.EnumerableNullOrEmpty())
				{
					float a = enumerable.Percent((Pair<IncidentDef, IncidentParms> i) => i.First.IsAnomalyIncident);
					a = Mathf.Max(a, 0f);
					stringBuilder.AppendLine(cat.defName + ": " + a.ToStringPercent());
				}
			}
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Incidents per StorytellerComp:");
		for (int num2 = 0; num2 < incCountsForComp.Length; num2++)
		{
			stringBuilder.AppendLine("  M" + num2.ToString().PadRight(5) + incCountsForComp[num2]);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Full incident record:");
		stringBuilder.Append(incidentList);
		Log.Message(stringBuilder.ToString());
	}

	public static void DebugGetFutureIncidents(int numTestDays, bool currentMapOnly, out Dictionary<IIncidentTarget, int> incCountsForTarget, out int[] incCountsForComp, out List<Pair<IncidentDef, IncidentParms>> allIncidents, out int threatBigCount, StringBuilder outputSb = null, StorytellerComp onlyThisComp = null, ThreatsGeneratorParams onlyThisThreatsGenerator = null, QuestPart onlyThisQuestPart = null)
	{
		int ticksGame = Find.TickManager.TicksGame;
		IncidentQueue incidentQueue = Find.Storyteller.incidentQueue;
		List<IIncidentTarget> allIncidentTargets = Find.Storyteller.AllIncidentTargets;
		tmpOldStoryStates.Clear();
		for (int i = 0; i < allIncidentTargets.Count; i++)
		{
			IIncidentTarget incidentTarget = allIncidentTargets[i];
			tmpOldStoryStates.Add(incidentTarget, incidentTarget.StoryState);
			new StoryState(incidentTarget).CopyTo(incidentTarget.StoryState);
		}
		Find.Storyteller.incidentQueue = new IncidentQueue();
		int num = numTestDays * 60;
		incCountsForComp = new int[Find.Storyteller.storytellerComps.Count];
		incCountsForTarget = new Dictionary<IIncidentTarget, int>();
		allIncidents = new List<Pair<IncidentDef, IncidentParms>>();
		threatBigCount = 0;
		for (int j = 0; j < num; j++)
		{
			IEnumerable<FiringIncident> enumerable = ((onlyThisThreatsGenerator != null) ? ThreatsGenerator.MakeIntervalIncidents(onlyThisThreatsGenerator, Find.CurrentMap, ticksGame) : ((onlyThisComp != null) ? Find.Storyteller.MakeIncidentsForInterval(onlyThisComp, Find.Storyteller.AllIncidentTargets) : ((onlyThisQuestPart == null) ? Find.Storyteller.MakeIncidentsForInterval() : (from x in Find.Storyteller.MakeIncidentsForInterval()
				where x.sourceQuestPart == onlyThisQuestPart
				select x))));
			foreach (FiringIncident item in enumerable)
			{
				if (item == null)
				{
					Log.Error("Null incident generated.");
				}
				if (currentMapOnly && item.parms.target != Find.CurrentMap)
				{
					continue;
				}
				item.parms.target.StoryState.Notify_IncidentFired(item);
				Find.Storyteller.RecordIncidentFired(item.def);
				allIncidents.Add(new Pair<IncidentDef, IncidentParms>(item.def, item.parms));
				if (!incCountsForTarget.ContainsKey(item.parms.target))
				{
					incCountsForTarget[item.parms.target] = 0;
				}
				incCountsForTarget[item.parms.target]++;
				string text;
				if (item.def.category != IncidentCategoryDefOf.ThreatBig)
				{
					text = ((item.def.category != IncidentCategoryDefOf.ThreatSmall) ? "  " : "S ");
				}
				else
				{
					threatBigCount++;
					text = "T ";
				}
				string text2;
				if (onlyThisThreatsGenerator != null)
				{
					text2 = "";
				}
				else
				{
					int num2 = Find.Storyteller.storytellerComps.IndexOf(item.source);
					if (num2 >= 0)
					{
						incCountsForComp[num2]++;
						text2 = "M" + num2 + " ";
					}
					else
					{
						text2 = "";
					}
				}
				text2 = text2.PadRight(4);
				outputSb?.AppendLine(text2 + text + (Find.TickManager.TicksGame.TicksToDays().ToString("F1") + "d").PadRight(6) + " " + item);
			}
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1000);
		}
		Find.TickManager.DebugSetTicksGame(ticksGame);
		Find.Storyteller.incidentQueue = incidentQueue;
		for (int num3 = 0; num3 < allIncidentTargets.Count; num3++)
		{
			tmpOldStoryStates[allIncidentTargets[num3]].CopyTo(allIncidentTargets[num3].StoryState);
		}
		tmpOldStoryStates.Clear();
	}

	public static void DebugLogTestIncidentTargets()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Available incident targets:\n");
		foreach (IIncidentTarget allIncidentTarget in Find.Storyteller.AllIncidentTargets)
		{
			stringBuilder.AppendLine(allIncidentTarget.ToString());
			foreach (IncidentTargetTagDef item in allIncidentTarget.IncidentTargetTags())
			{
				stringBuilder.AppendLine("  " + item);
			}
			stringBuilder.AppendLine("");
		}
		Log.Message(stringBuilder.ToString());
	}
}
