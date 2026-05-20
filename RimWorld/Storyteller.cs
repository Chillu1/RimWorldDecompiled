using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Storyteller : IExposable
{
	public StorytellerDef def;

	public DifficultyDef difficultyDef;

	public Difficulty difficulty = new Difficulty();

	public List<StorytellerComp> storytellerComps;

	private static List<IncidentDef> anomalyIncidents;

	private static List<IncidentDef> nonAnomalyIncidents;

	public IncidentQueue incidentQueue = new IncidentQueue();

	private Queue<bool> recentIncidentsAnomaly = new Queue<bool>();

	private float recentAnomalyIncidentFactor = 0.5f;

	private int lastQuestGiverTraderTick;

	private int lastIncidentTick;

	private float anomalyThreatsInactiveFraction = 0.08f;

	private float anomalyThreatsActiveFraction = 0.3f;

	private float? overrideAnomalyThreatsFraction;

	private float studyEfficiencyFactor = 1f;

	private AnomalyPlaystyleDef anomalyPlaystyleDef;

	public static readonly Vector2 PortraitSizeTiny = new Vector2(122f, 130f);

	public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

	public const int IntervalsPerDay = 60;

	public const int CheckInterval = 1000;

	private const int RecentAnomalyIncidentsToRecord = 4;

	private const float QuestGiverTraderChance = 0.15f;

	private const int QuestGiverTraderMinIntervalTicks = 600000;

	private const int QuestGiverTraderMaxIntervalTicks = 3600000;

	private static readonly List<IIncidentTarget> tmpAllIncidentTargets = new List<IIncidentTarget>();

	private string debugStringCached = "Generating data...";

	[Obsolete("Use \"difficulty\" instead.")]
	public Difficulty difficultyValues => difficulty;

	public int LastIncidentTick => lastIncidentTick;

	public List<IIncidentTarget> AllIncidentTargets
	{
		get
		{
			tmpAllIncidentTargets.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				tmpAllIncidentTargets.Add(maps[i]);
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				if (caravans[j].IsPlayerControlled)
				{
					tmpAllIncidentTargets.Add(caravans[j]);
				}
			}
			tmpAllIncidentTargets.Add(Find.World);
			return tmpAllIncidentTargets;
		}
	}

	public static List<IncidentDef> AnomalyIncidents => anomalyIncidents ?? (anomalyIncidents = DefDatabase<IncidentDef>.AllDefsListForReading.Where((IncidentDef incident) => incident.IsAnomalyIncident).ToList());

	public static List<IncidentDef> NonAnomalyIncidents => nonAnomalyIncidents ?? (nonAnomalyIncidents = DefDatabase<IncidentDef>.AllDefsListForReading.Where((IncidentDef incident) => !incident.IsAnomalyIncident).ToList());

	public float AnomalyIncidentChanceNow => Mathf.Clamp01(Find.Anomaly.AnomalyThreatFractionNow);

	public float QuestGiverTraderChanceNow
	{
		get
		{
			if (Find.TickManager.TicksGame - lastQuestGiverTraderTick < 600000)
			{
				return 0f;
			}
			if (Find.TickManager.TicksGame - lastQuestGiverTraderTick > 3600000)
			{
				return 1f;
			}
			return 0.15f;
		}
	}

	public static void StorytellerStaticUpdate()
	{
		tmpAllIncidentTargets.Clear();
	}

	public Storyteller()
	{
	}

	public Storyteller(StorytellerDef def, DifficultyDef difficultyDef)
		: this(def, difficultyDef, new Difficulty(difficultyDef))
	{
	}

	public Storyteller(StorytellerDef def, DifficultyDef difficultyDef, Difficulty difficulty)
	{
		this.def = def;
		this.difficultyDef = difficultyDef;
		this.difficulty = difficulty;
		InitializeStorytellerComps();
	}

	private void InitializeStorytellerComps()
	{
		storytellerComps = new List<StorytellerComp>();
		for (int i = 0; i < def.comps.Count; i++)
		{
			if (def.comps[i].Enabled)
			{
				StorytellerComp storytellerComp = (StorytellerComp)Activator.CreateInstance(def.comps[i].compClass);
				storytellerComp.props = def.comps[i];
				storytellerComp.Initialize();
				storytellerComps.Add(storytellerComp);
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Defs.Look(ref difficultyDef, "difficulty");
		Scribe_Deep.Look(ref incidentQueue, "incidentQueue");
		Scribe_Collections.Look(ref recentIncidentsAnomaly, "recentIncidentsAnomaly", LookMode.Value);
		Scribe_Values.Look(ref recentAnomalyIncidentFactor, "recentAnomalyIncidentFactor", 0.5f);
		Scribe_Values.Look(ref lastQuestGiverTraderTick, "lastQuestGiverTraderTick", 0);
		Scribe_Values.Look(ref lastIncidentTick, "lastIncidentTick", 0);
		if (difficultyDef == null)
		{
			Log.Error("Loaded storyteller without difficulty");
			difficultyDef = DifficultyDefOf.Rough;
		}
		if (difficultyDef.isCustom)
		{
			Scribe_Deep.Look(ref difficulty, "customDifficulty", difficultyDef);
		}
		else if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			difficulty = new Difficulty(difficultyDef);
		}
		if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
		{
			InitializeStorytellerComps();
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.Saving)
		{
			SetupNonCustomDifficultyAnomalySettings();
		}
	}

	private void SetupNonCustomDifficultyAnomalySettings()
	{
		if (ModsConfig.AnomalyActive && !difficultyDef.isCustom)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Scribe_Values.Look(ref difficulty.anomalyThreatsInactiveFraction, "anomalyThreatsInactiveFraction", 0.08f);
				Scribe_Values.Look(ref difficulty.anomalyThreatsActiveFraction, "anomalyThreatsActiveFraction", 0.3f);
				Scribe_Values.Look(ref difficulty.overrideAnomalyThreatsFraction, "overrideAnomalyThreatsFraction");
				Scribe_Values.Look(ref difficulty.studyEfficiencyFactor, "studyEfficiencyFactor", 1f);
				Scribe_Defs.Look(ref anomalyPlaystyleDef, "anomalyPlaystyleDef");
				difficulty.AnomalyPlaystyleDef = anomalyPlaystyleDef;
			}
			else if (Scribe.mode == LoadSaveMode.Saving)
			{
				anomalyThreatsInactiveFraction = difficulty.anomalyThreatsInactiveFraction;
				anomalyThreatsActiveFraction = difficulty.anomalyThreatsActiveFraction;
				overrideAnomalyThreatsFraction = difficulty.overrideAnomalyThreatsFraction;
				studyEfficiencyFactor = difficulty.studyEfficiencyFactor;
				anomalyPlaystyleDef = difficulty.AnomalyPlaystyleDef;
				Scribe_Values.Look(ref anomalyThreatsInactiveFraction, "anomalyThreatsInactiveFraction", 0.08f);
				Scribe_Values.Look(ref anomalyThreatsActiveFraction, "anomalyThreatsActiveFraction", 0.3f);
				Scribe_Values.Look(ref overrideAnomalyThreatsFraction, "overrideAnomalyThreatsFraction");
				Scribe_Values.Look(ref studyEfficiencyFactor, "studyEfficiencyFactor", 1f);
				Scribe_Defs.Look(ref anomalyPlaystyleDef, "anomalyPlaystyleDef");
			}
		}
	}

	public void StorytellerTick()
	{
		incidentQueue.IncidentQueueTick();
		if (Find.TickManager.TicksGame % 1000 != 0 || !DebugSettings.enableStoryteller)
		{
			return;
		}
		foreach (FiringIncident item in MakeIncidentsForInterval())
		{
			TryFire(item);
		}
	}

	public bool TryFire(FiringIncident fi, bool queued = false)
	{
		if (fi.def.Worker.CanFireNow(fi.parms) && fi.def.Worker.TryExecute(fi.parms))
		{
			fi.parms.target.StoryState.Notify_IncidentFired(fi);
			lastIncidentTick = GenTicks.TicksGame;
			return true;
		}
		return false;
	}

	public IEnumerable<FiringIncident> MakeIncidentsForInterval()
	{
		List<IIncidentTarget> targets = AllIncidentTargets;
		for (int i = 0; i < storytellerComps.Count; i++)
		{
			foreach (FiringIncident item in MakeIncidentsForInterval(storytellerComps[i], targets))
			{
				yield return item;
			}
		}
		List<Quest> quests = Find.QuestManager.QuestsListForReading;
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].State != QuestState.Ongoing)
			{
				continue;
			}
			List<QuestPart> parts = quests[i].PartsListForReading;
			for (int j = 0; j < parts.Count; j++)
			{
				if (!(parts[j] is IIncidentMakerQuestPart incidentMakerQuestPart) || ((QuestPartActivable)parts[j]).State != QuestPartState.Enabled)
				{
					continue;
				}
				foreach (FiringIncident item2 in incidentMakerQuestPart.MakeIntervalIncidents())
				{
					item2.sourceQuestPart = parts[j];
					item2.parms.quest = quests[i];
					yield return item2;
				}
			}
		}
	}

	public IEnumerable<FiringIncident> MakeIncidentsForInterval(StorytellerComp comp, List<IIncidentTarget> targets)
	{
		if (GenDate.DaysPassedSinceSettleFloat <= comp.props.minDaysPassed)
		{
			yield break;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			IIncidentTarget incidentTarget = targets[i];
			bool flag = false;
			bool flag2 = comp.props.allowedTargetTags.NullOrEmpty();
			foreach (IncidentTargetTagDef item in incidentTarget.IncidentTargetTags())
			{
				if (!comp.props.disallowedTargetTags.NullOrEmpty() && comp.props.disallowedTargetTags.Contains(item))
				{
					flag = true;
					break;
				}
				if (!flag2 && comp.props.allowedTargetTags.Contains(item))
				{
					flag2 = true;
				}
			}
			if (flag || !flag2)
			{
				continue;
			}
			foreach (FiringIncident item2 in comp.MakeIntervalIncidents(incidentTarget))
			{
				if ((Find.Storyteller.difficulty.allowBigThreats || item2.def.category != IncidentCategoryDefOf.ThreatBig) && (!ModsConfig.AnomalyActive || Find.TickManager.TicksGame - Find.Anomaly.metalHellClosedTick >= 300000 || item2.def.category != IncidentCategoryDefOf.ThreatBig))
				{
					yield return item2;
				}
			}
		}
	}

	public void Notify_PawnEvent(Pawn pawn, AdaptationEvent ev, DamageInfo? dinfo = null)
	{
		Find.StoryWatcher.watcherAdaptation.Notify_PawnEvent(pawn, ev, dinfo);
		for (int i = 0; i < storytellerComps.Count; i++)
		{
			storytellerComps[i].Notify_PawnEvent(pawn, ev, dinfo);
		}
	}

	public void Notify_DissolutionEvent(Thing thing)
	{
		for (int i = 0; i < storytellerComps.Count; i++)
		{
			storytellerComps[i].Notify_DissolutionEvent(thing);
		}
	}

	public void Notify_QuestGiverTraderSpawned()
	{
		lastQuestGiverTraderTick = Find.TickManager.TicksGame;
	}

	public void Notify_DefChanged()
	{
		InitializeStorytellerComps();
	}

	public void RecordIncidentFired(IncidentDef incident)
	{
		if (ModsConfig.AnomalyActive && incident.category.canUseAnomalyChance)
		{
			if (recentIncidentsAnomaly == null)
			{
				recentIncidentsAnomaly = new Queue<bool>();
			}
			if (recentIncidentsAnomaly.Count >= 4)
			{
				recentIncidentsAnomaly.Dequeue();
			}
			recentIncidentsAnomaly.Enqueue(incident.IsAnomalyIncident);
			recentAnomalyIncidentFactor = recentIncidentsAnomaly.Average((bool b) => (!b) ? 0f : 1f);
		}
	}

	public static void ResetStaticData()
	{
		anomalyIncidents = null;
		nonAnomalyIncidents = null;
	}

	public string DebugString()
	{
		if (Time.frameCount % 60 == 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("GLOBAL STORYTELLER STATS");
			stringBuilder.AppendLine("------------------------");
			stringBuilder.AppendLine("Storyteller: ".PadRight(40) + def.label);
			stringBuilder.AppendLine("Adaptation days: ".PadRight(40) + Find.StoryWatcher.watcherAdaptation.AdaptDays.ToString("F1"));
			stringBuilder.AppendLine("Adapt points factor: ".PadRight(40) + Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor.ToString("F2"));
			stringBuilder.AppendLine("Time points factor: ".PadRight(40) + Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate(GenDate.DaysPassedSinceSettle).ToString("F2"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Ally incident fraction (neutral or ally): ".PadRight(40) + StorytellerUtility.AllyIncidentFraction(fullAlliesOnly: false).ToString("F2"));
			stringBuilder.AppendLine("Ally incident fraction (ally only): ".PadRight(40) + StorytellerUtility.AllyIncidentFraction(fullAlliesOnly: true).ToString("F2"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(StorytellerUtilityPopulation.DebugReadout().TrimEndNewlines());
			IIncidentTarget incidentTarget = (Find.WorldSelector.SingleSelectedObject as IIncidentTarget) ?? Find.CurrentMap;
			if (incidentTarget != null)
			{
				Map map = incidentTarget as Map;
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("STATS FOR INCIDENT TARGET: " + incidentTarget);
				stringBuilder.AppendLine("------------------------");
				stringBuilder.AppendLine("Progress score: ".PadRight(40) + StorytellerUtility.GetProgressScore(incidentTarget).ToString("F2"));
				stringBuilder.AppendLine("Base points: ".PadRight(40) + StorytellerUtility.DefaultThreatPointsNow(incidentTarget).ToString("F0"));
				stringBuilder.AppendLine("Points factor random range: ".PadRight(40) + incidentTarget.IncidentPointsRandomFactorRange.ToString());
				stringBuilder.AppendLine("Wealth: ".PadRight(40) + incidentTarget.PlayerWealthForStoryteller.ToString("F0"));
				if (map != null)
				{
					if (Find.Storyteller.difficulty.fixedWealthMode)
					{
						stringBuilder.AppendLine($"- Wealth calculated using fixed model curve, time factor: {Find.Storyteller.difficulty.fixedWealthTimeFactor:F1}");
						stringBuilder.AppendLine("- Map age: ".PadRight(40) + map.AgeInDays.ToString("F1"));
					}
					stringBuilder.AppendLine("- Items: ".PadRight(40) + map.wealthWatcher.WealthItems.ToString("F0"));
					stringBuilder.AppendLine("- Buildings: ".PadRight(40) + map.wealthWatcher.WealthBuildings.ToString("F0"));
					stringBuilder.AppendLine("- Floors: ".PadRight(40) + map.wealthWatcher.WealthFloorsOnly.ToString("F0"));
					stringBuilder.AppendLine("- Pawns: ".PadRight(40) + map.wealthWatcher.WealthPawns.ToString("F0"));
				}
				stringBuilder.AppendLine("Pawn count human: ".PadRight(40) + incidentTarget.PlayerPawnsForStoryteller.Count((Pawn p) => p.def.race.Humanlike));
				stringBuilder.AppendLine("Pawn count animal: ".PadRight(40) + incidentTarget.PlayerPawnsForStoryteller.Count((Pawn p) => p.IsAnimal));
				if (map != null)
				{
					stringBuilder.AppendLine("StoryDanger: ".PadRight(40) + map.dangerWatcher.DangerRating);
					stringBuilder.AppendLine("FireDanger: ".PadRight(40) + map.fireWatcher.FireDanger.ToString("F2"));
					stringBuilder.AppendLine("LastThreatBigTick days ago: ".PadRight(40) + (Find.TickManager.TicksGame - map.storyState.LastThreatBigTick).ToStringTicksToDays());
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("LIST OF ALL INCIDENT TARGETS");
			stringBuilder.AppendLine("------------------------");
			for (int num = 0; num < AllIncidentTargets.Count; num++)
			{
				stringBuilder.AppendLine(num + ". " + AllIncidentTargets[num]);
			}
			debugStringCached = stringBuilder.ToString();
		}
		return debugStringCached;
	}
}
