using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class GameComponent_Anomaly : GameComponent
{
	public const string MonolithLevelChangedSignal = "MonolithLevelChanged";

	private const int GrayPallDelayTicks = 300;

	private const int LevelChangeScreenShakeDuration = 300;

	private const int MinTimeBetweenNewMetalhorrorBiosignatureImplants = 1800000;

	private const int MonolithStudyNoteLetters = 3;

	private const float LevelChangeScreenShakeMagnitude = 0.05f;

	private const float HarbingerTreeRespawnIntervalDays = 8f;

	public const int PostEndGameReliefPeriod = 300000;

	private static readonly FloatRange MonolithLevelIncidentDelayRangeHours = new FloatRange(12f, 36f);

	private static readonly FloatRange HarbingerTreeCheckIntervalHours = new FloatRange(6f, 18f);

	private static readonly FloatRange HarbingerTreeSpawnIntervalDays = new FloatRange(2f, 5f);

	private static readonly FloatRange GrayPallConditionDaysRange = new FloatRange(1f, 3f);

	private static readonly FloatRange VoidCuriosityIncidentDelayRangeDays = new FloatRange(15f, 20f);

	private int level;

	private int highestLevelReached;

	private MonolithLevelDef levelDef;

	public Building_VoidMonolith monolith;

	private int fireLevelIncidentTick = -99999;

	private int nextHarbingerTreeCheckTick = -99999;

	private int lastLevelChangeTick = -99999;

	private int fireGrayPallTick = -99999;

	private int newestMetalhorrorBiosignatureTick = -1800000;

	public bool hasBuiltHoldingPlatform;

	public bool hasSeenGrayFlesh;

	private Dictionary<Pawn, UnnaturalCorpseTracker> corpseTrackers = new Dictionary<Pawn, UnnaturalCorpseTracker>();

	private List<UnnaturalCorpseTracker> looseCorpseTrackers = new List<UnnaturalCorpseTracker>();

	private Dictionary<Pawn, int> hypnotisedPawns = new Dictionary<Pawn, int>();

	public HashSet<int> emergedBiosignatures = new HashSet<int>();

	public bool hasPerformedVoidProvocation;

	public int lastLevelActivationLetterSent = -1;

	private List<ChoiceLetter> monolithLetters = new List<ChoiceLetter>();

	private int monolithNextIndex;

	private int monolithStudyProgress;

	public float monolithAnomalyKnowledge;

	public Pawn voidNodeActivator;

	public List<Pawn> metalHellPawns = new List<Pawn>();

	public int metalHellReturnTick = -99999;

	public int metalHellClosedTick = -99999;

	public string metalHellReturnLetterText;

	private LargeBuildingSpawnParms monolithSpawnParms;

	private static readonly List<Pawn> toRemove = new List<Pawn>();

	private static readonly List<UnnaturalCorpseTracker> tmpTrackers = new List<UnnaturalCorpseTracker>();

	private List<Pawn> workingPawnList;

	private List<UnnaturalCorpseTracker> workingTrackerList;

	private List<Pawn> workingHypnotizedList;

	private List<int> workingHypnotizedTickList;

	public bool MonolithSpawned
	{
		get
		{
			if (monolith != null)
			{
				return monolith.Spawned;
			}
			return false;
		}
	}

	public LargeBuildingSpawnParms MonolithSpawnParms => monolithSpawnParms;

	public int Level
	{
		get
		{
			if (!MonolithSpawned)
			{
				return 0;
			}
			return level;
		}
	}

	public int HighestLevelReached => highestLevelReached;

	public MonolithLevelDef LevelDef
	{
		get
		{
			if (!MonolithSpawned)
			{
				return MonolithLevelDefOf.Inactive;
			}
			return levelDef ?? MonolithLevelDefOf.Inactive;
		}
	}

	public MonolithLevelDef NextLevelDef
	{
		get
		{
			if (!LevelDef.advanceThroughActivation)
			{
				return null;
			}
			return DefDatabase<MonolithLevelDef>.AllDefs.FirstOrDefault((MonolithLevelDef x) => x.level == Level + 1);
		}
	}

	public bool QuestlineEnded
	{
		get
		{
			if (LevelDef != MonolithLevelDefOf.Embraced)
			{
				return LevelDef == MonolithLevelDefOf.Disrupted;
			}
			return true;
		}
	}

	public int TicksSinceLastLevelChange => Find.TickManager.TicksGame - lastLevelChangeTick;

	private int TicksSinceLastMetalhorrorBio => Find.TickManager.TicksGame - newestMetalhorrorBiosignatureTick;

	public bool CanNewMetalhorrorBiosignatureImplantOccur => TicksSinceLastMetalhorrorBio >= 1800000;

	public IReadOnlyList<ChoiceLetter> MonolithLetters => monolithLetters;

	public bool MonolithStudyCompleted => monolithLetters.Count == 3;

	public int MonolithNextIndex => monolithNextIndex;

	public int MonolithStudyProgress => monolithStudyProgress;

	public bool GenerateMonolith => Find.Storyteller.difficulty.AnomalyPlaystyleDef.generateMonolith;

	public bool AmbientHorrorMode => Find.Storyteller.difficulty.AnomalyPlaystyleDef == AnomalyPlaystyleDefOf.AmbientHorror;

	public bool AnomalyStudyEnabled
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			if (!Find.Storyteller.difficulty.AnomalyPlaystyleDef.enableAnomalyContent)
			{
				return false;
			}
			if (HighestLevelReached > 0)
			{
				return true;
			}
			if (!GenerateMonolith)
			{
				return true;
			}
			return false;
		}
	}

	public float AnomalyThreatFractionNow
	{
		get
		{
			AnomalyPlaystyleDef anomalyPlaystyleDef = Find.Storyteller.difficulty.AnomalyPlaystyleDef;
			if (!anomalyPlaystyleDef.enableAnomalyContent)
			{
				return 0f;
			}
			if (anomalyPlaystyleDef.overrideThreatFraction && Find.Storyteller.difficulty.overrideAnomalyThreatsFraction.HasValue)
			{
				return Find.Storyteller.difficulty.overrideAnomalyThreatsFraction.Value;
			}
			MonolithLevelDef monolithLevelDef = LevelDef;
			if (monolithLevelDef.useInactiveAnomalyThreatFraction)
			{
				return Find.Storyteller.difficulty.anomalyThreatsInactiveFraction * monolithLevelDef.anomalyThreatFractionFactor;
			}
			if (monolithLevelDef.useActiveAnomalyThreatFraction)
			{
				return Find.Storyteller.difficulty.anomalyThreatsActiveFraction * monolithLevelDef.anomalyThreatFractionFactor;
			}
			return LevelDef.anomalyThreatFraction * monolithLevelDef.anomalyThreatFractionFactor;
		}
	}

	public GameComponent_Anomaly(Game game)
	{
		if (ModsConfig.AnomalyActive)
		{
			levelDef = DefDatabase<MonolithLevelDef>.AllDefs.FirstOrDefault((MonolithLevelDef x) => x.level == level);
			level = levelDef.level;
			monolithSpawnParms = new LargeBuildingSpawnParms
			{
				minDistanceToColonyBuilding = 30f,
				minDistToEdge = 10,
				attemptSpawnLocationType = SpawnLocationType.Outdoors,
				attemptNotUnderBuildings = true,
				canSpawnOnImpassable = false,
				allowFogged = true,
				overrideSize = new IntVec2(ThingDefOf.VoidMonolith.size.x + 2, ThingDefOf.VoidMonolith.size.z + 2)
			};
		}
	}

	public override void StartedNewGame()
	{
		base.StartedNewGame();
		if (ModsConfig.AnomalyActive)
		{
			Notify_LevelChanged(silent: true);
			nextHarbingerTreeCheckTick = Find.TickManager.TicksGame + Mathf.RoundToInt(HarbingerTreeCheckIntervalHours.RandomInRange * 2500f);
			monolith = Find.AnyPlayerHomeMap.listerThings.ThingsOfDef(ThingDefOf.VoidMonolith).FirstOrDefault() as Building_VoidMonolith;
		}
	}

	public override void GameComponentTick()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return;
		}
		if (monolith != null && monolith.quest == null && Level > 0)
		{
			monolith.CheckAndGenerateQuest();
		}
		if (monolith != null && !monolith.Spawned)
		{
			monolith = null;
		}
		if (Find.TickManager.TicksGame > nextHarbingerTreeCheckTick)
		{
			TrySpawnHarbingerTrees();
			nextHarbingerTreeCheckTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + HarbingerTreeCheckIntervalHours.RandomInRange * 2500f);
		}
		UpdateCorpseTrackers();
		UpdateHypnotized();
		if (fireLevelIncidentTick > 0 && Find.TickManager.TicksGame > fireLevelIncidentTick)
		{
			if (levelDef.incidentsOnReached != null)
			{
				foreach (IncidentDef item in levelDef.incidentsOnReached)
				{
					IncidentParms parms = new IncidentParms
					{
						target = Find.AnyPlayerHomeMap,
						points = StorytellerUtility.DefaultThreatPointsNow(monolith.Map),
						forced = true
					};
					Find.Storyteller.incidentQueue.Add(item, Find.TickManager.TicksGame, parms);
				}
			}
			fireLevelIncidentTick = -99999;
		}
		if (fireGrayPallTick > 0 && GenTicks.TicksGame > fireGrayPallTick)
		{
			if (monolith != null)
			{
				int duration = Mathf.CeilToInt(GrayPallConditionDaysRange.RandomInRange * 60000f);
				GameCondition cond = GameConditionMaker.MakeCondition(GameConditionDefOf.GrayPall, duration);
				monolith.MapHeld.GameConditionManager.RegisterCondition(cond);
				TaggedString label = "LetterLabelGrayPallDescending".Translate();
				TaggedString text = "LetterGrayPallDescending".Translate();
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);
			}
			fireGrayPallTick = -99999;
		}
		if (!metalHellPawns.NullOrEmpty() && Find.TickManager.TicksGame > metalHellReturnTick)
		{
			Map map = monolith.Map;
			foreach (Pawn metalHellPawn in metalHellPawns)
			{
				if (CellFinder.TryFindRandomCellNear(monolith.Position, monolith.Map, 5, (IntVec3 c) => !c.Fogged(map) && c.Standable(map), out var result))
				{
					SkipUtility.SkipTo(metalHellPawn, result, monolith.Map);
				}
				else if (CellFinder.TryFindRandomCell(monolith.Map, (IntVec3 c) => !c.Fogged(map) && c.Standable(map) && map.reachability.CanReachColony(c), out result))
				{
					SkipUtility.SkipTo(metalHellPawn, result, monolith.Map);
				}
				monolith.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(result, monolith.Map), result, 60);
				SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(result, monolith.Map));
				Messages.Message("VoidNodeReturnedMessage".Translate(metalHellPawn.Named("PAWN")), metalHellPawn, MessageTypeDefOf.PositiveEvent);
			}
			Find.LetterStack.ReceiveLetter("VoidNodeReturnedLabel".Translate(voidNodeActivator.Named("PAWN")), metalHellReturnLetterText, LetterDefOf.PositiveEvent, voidNodeActivator);
			metalHellPawns.Clear();
			metalHellReturnTick = -99999;
			metalHellReturnLetterText = null;
		}
		if (Find.TickManager.TicksGame != metalHellClosedTick + 60 || levelDef != MonolithLevelDefOf.Disrupted)
		{
			return;
		}
		foreach (Faction item2 in Find.FactionManager.AllFactionsVisible)
		{
			if (item2 != Faction.OfPlayer && item2.def.humanlikeFaction && !item2.def.permanentEnemy)
			{
				item2.TryAffectGoodwillWith(Faction.OfPlayer, 50, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DestroyedVoidMonolith);
			}
		}
	}

	public void Notify_NewMetalhorrorBiosignatureImplanted()
	{
		newestMetalhorrorBiosignatureTick = GenTicks.TicksGame;
	}

	public void Notify_MonolithStudyIncreased(ChoiceLetter letter, int nextIndex, int studyProgress)
	{
		monolithLetters.Add(letter);
		monolithNextIndex = nextIndex;
		monolithStudyProgress = studyProgress;
	}

	private void UpdateHypnotized()
	{
		foreach (var (item, num2) in hypnotisedPawns)
		{
			if (GenTicks.TicksGame > num2)
			{
				toRemove.Add(item);
			}
		}
		foreach (Pawn item2 in toRemove)
		{
			hypnotisedPawns.Remove(item2);
		}
		toRemove.Clear();
	}

	private void UpdateCorpseTrackers()
	{
		ElevateLooseCorpseTrackers();
		foreach (UnnaturalCorpseTracker looseCorpseTracker in looseCorpseTrackers)
		{
			looseCorpseTracker.CorpseTick();
			if (looseCorpseTracker.ShouldDisappear)
			{
				looseCorpseTracker.Notify_Finished();
				tmpTrackers.Add(looseCorpseTracker);
			}
		}
		foreach (var (item, unnaturalCorpseTracker2) in corpseTrackers)
		{
			unnaturalCorpseTracker2.CorpseTick();
			if (unnaturalCorpseTracker2.ShouldDisappear)
			{
				unnaturalCorpseTracker2.Notify_Finished();
				toRemove.Add(item);
			}
		}
		foreach (Pawn item2 in toRemove)
		{
			corpseTrackers.Remove(item2);
		}
		foreach (UnnaturalCorpseTracker tmpTracker in tmpTrackers)
		{
			looseCorpseTrackers.Remove(tmpTracker);
		}
		toRemove.Clear();
		tmpTrackers.Clear();
	}

	public void RemoveCorpseTracker(Pawn pawn)
	{
		if (corpseTrackers.TryGetValue(pawn, out var value))
		{
			value.Notify_Finished();
			corpseTrackers.Remove(pawn);
		}
	}

	public void IncrementLevel()
	{
		if (LevelDef == MonolithLevelDefOf.Inactive)
		{
			level = 0;
		}
		level++;
		Notify_LevelChanged();
	}

	public void Hypnotize(Pawn pawn, Pawn instigator, int ticks)
	{
		Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_Hypnotized, instigator));
		hypnotisedPawns[pawn] = GenTicks.TicksGame + ticks;
	}

	public void EndHypnotize(Pawn pawn)
	{
		hypnotisedPawns.Remove(pawn);
	}

	public bool IsPawnHypnotized(Pawn pawn)
	{
		return hypnotisedPawns.ContainsKey(pawn);
	}

	public void SetLevel(MonolithLevelDef levelDef, bool silent = false)
	{
		int num = level;
		level = levelDef.level;
		if (num != level)
		{
			Notify_LevelChanged(silent);
		}
	}

	private void Notify_LevelChanged(bool silent = false)
	{
		if (!ModLister.CheckAnomaly("Monolith level"))
		{
			return;
		}
		highestLevelReached = Mathf.Max(highestLevelReached, level);
		lastLevelChangeTick = Find.TickManager.TicksGame;
		levelDef = DefDatabase<MonolithLevelDef>.AllDefs.FirstOrDefault((MonolithLevelDef x) => x.level == level);
		monolith?.SetLevel(levelDef);
		Find.ResearchManager.Notify_MonolithLevelChanged(level);
		if (!silent)
		{
			if (!levelDef.incidentsOnReached.NullOrEmpty())
			{
				fireLevelIncidentTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + MonolithLevelIncidentDelayRangeHours.RandomInRange * 2500f);
			}
			if (Level > 0)
			{
				Find.CameraDriver.shaker.DoShake(0.05f, 300);
			}
			if (LevelDef.triggersGrayPall)
			{
				TriggerGrayPall();
			}
			if (LevelDef == MonolithLevelDefOf.VoidAwakened)
			{
				TriggerVoidAwakening();
			}
		}
		Find.SignalManager.SendSignal(new Signal("MonolithLevelChanged", global: true));
		if (level == 1)
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.StudyingEntities, OpportunityType.Important);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.EntityCodex, OpportunityType.GoodToKnow);
			IncidentParms parms = new IncidentParms
			{
				target = monolith.Map,
				points = StorytellerUtility.DefaultThreatPointsNow(monolith.Map),
				forced = true
			};
			Find.Storyteller.incidentQueue.Add(IncidentDefOf.VoidCuriosity, Find.TickManager.TicksGame + Mathf.RoundToInt(VoidCuriosityIncidentDelayRangeDays.RandomInRange * 60000f), parms);
		}
	}

	private void TriggerGrayPall()
	{
		fireGrayPallTick = GenTicks.TicksGame + 300;
	}

	private void TriggerVoidAwakening()
	{
		Slate slate = new Slate();
		slate.Set("map", monolith.Map);
		QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.EndGame_VoidAwakening, slate);
	}

	public bool TryGetCellForMonolithSpawn(Map map, out IntVec3 cell, bool allowFogged = false)
	{
		LargeBuildingSpawnParms largeBuildingSpawnParms = MonolithSpawnParms.ForThing(ThingDefOf.VoidMonolith);
		LargeBuildingSpawnParms parms = largeBuildingSpawnParms;
		parms.minDistanceToColonyBuilding = 1f;
		largeBuildingSpawnParms.allowFogged = allowFogged;
		parms.allowFogged = allowFogged;
		if (!LargeBuildingCellFinder.TryFindCell(out cell, map, largeBuildingSpawnParms, null, (IntVec3 c) => ScattererValidator_AvoidSpecialThings.IsValid(c, map)) && !LargeBuildingCellFinder.TryFindCell(out cell, map, parms, null, (IntVec3 c) => ScattererValidator_AvoidSpecialThings.IsValid(c, map)))
		{
			return false;
		}
		return true;
	}

	public Building_VoidMonolith SpawnNewMonolith(IntVec3 cell, Map map)
	{
		monolith = ThingMaker.MakeThing(ThingDefOf.VoidMonolith) as Building_VoidMonolith;
		levelDef = DefDatabase<MonolithLevelDef>.AllDefs.FirstOrDefault((MonolithLevelDef x) => x.level == level);
		GenSpawn.Spawn(monolith, cell, map);
		EffecterDefOf.VoidStructureSpawningSkipSequence.SpawnMaintained(monolith, map);
		if (Level > 0)
		{
			monolith.CheckAndGenerateQuest();
		}
		return monolith;
	}

	public void ResetMonolith()
	{
		SetLevel(MonolithLevelDefOf.Inactive);
		monolith?.Reset();
		fireLevelIncidentTick = -99999;
		lastLevelChangeTick = -99999;
	}

	public bool VoidAwakeningActive()
	{
		foreach (Quest item in Find.QuestManager.questsInDisplayOrder)
		{
			if (!item.Historical && item.root == QuestScriptDefOf.EndGame_VoidAwakening)
			{
				return true;
			}
		}
		return false;
	}

	public bool PawnHasUnnaturalCorpse(Pawn pawn)
	{
		return corpseTrackers.ContainsKey(pawn);
	}

	public bool TryGetUnnaturalCorpseTrackerForHaunted(Pawn pawn, out UnnaturalCorpseTracker tracker)
	{
		return corpseTrackers.TryGetValue(pawn, out tracker);
	}

	public bool TryGetUnnaturalCorpseTrackerForAwoken(Pawn pawn, out UnnaturalCorpseTracker tracker)
	{
		foreach (var (_, unnaturalCorpseTracker2) in corpseTrackers)
		{
			if (unnaturalCorpseTracker2.AwokenPawn == pawn)
			{
				tracker = unnaturalCorpseTracker2;
				return true;
			}
		}
		tracker = null;
		return false;
	}

	public bool HasActiveAwokenCorpse()
	{
		foreach (var (_, unnaturalCorpseTracker2) in corpseTrackers)
		{
			if (!unnaturalCorpseTracker2.AwokenPawn.DestroyedOrNull() && unnaturalCorpseTracker2.AwokenPawn.Spawned && !unnaturalCorpseTracker2.Haunted.DestroyedOrNull() && !unnaturalCorpseTracker2.Haunted.Dead && unnaturalCorpseTracker2.Haunted.MapHeld == unnaturalCorpseTracker2.AwokenPawn.Map)
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterUnnaturalCorpse(Pawn pawn, UnnaturalCorpse corpse)
	{
		if (corpseTrackers.ContainsKey(pawn))
		{
			Log.Error("Attempted to register a pawn (" + pawn.LabelShort + ") which already has an unnatural corpse");
			return;
		}
		corpseTrackers[pawn] = new UnnaturalCorpseTracker(pawn, corpse);
		Find.EntityCodex.SetDiscovered(EntityCodexEntryDefOf.UnnaturalCorpse);
	}

	public void DevRemoveUnnaturalCorpse(Pawn pawn)
	{
		if (corpseTrackers.ContainsKey(pawn))
		{
			corpseTrackers[pawn].Notify_Finished();
			corpseTrackers.Remove(pawn);
		}
	}

	public void Notify_MapRemoved(Map map)
	{
		if (monolith == null || monolith.Map != map)
		{
			return;
		}
		if (monolith.quest != null && !monolith.quest.Historical)
		{
			monolith.quest.End(QuestEndOutcome.Unknown);
		}
		if (levelDef == MonolithLevelDefOf.VoidAwakened || levelDef == MonolithLevelDefOf.Gleaming)
		{
			SetLevel(MonolithLevelDefOf.Waking, silent: true);
		}
		foreach (Pawn metalHellPawn in metalHellPawns)
		{
			metalHellPawn.Destroy();
		}
		metalHellPawns.Clear();
		metalHellReturnTick = -99999;
		monolith = null;
		fireLevelIncidentTick = -99999;
	}

	public void Notify_PawnDied(Pawn pawn)
	{
		if (corpseTrackers.TryGetValue(pawn, out var value))
		{
			value.Notify_PawnDied();
		}
	}

	public void Notify_PawnKilledViaAwoken(Pawn pawn)
	{
		if (corpseTrackers.TryGetValue(pawn, out var value))
		{
			value.Notify_PawnKilledViaAwoken();
		}
	}

	private void TrySpawnHarbingerTrees()
	{
		IncidentParms parms = new IncidentParms
		{
			target = Find.AnyPlayerHomeMap,
			forced = true
		};
		Find.Storyteller.incidentQueue.Add(IncidentDefOf.HarbingerTreeSpawn, Find.TickManager.TicksGame, parms);
	}

	public void Notify_HarbingerTreeSpawned()
	{
		nextHarbingerTreeCheckTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + HarbingerTreeSpawnIntervalDays.RandomInRange * 60000f + HarbingerTreeCheckIntervalHours.RandomInRange * 2500f);
	}

	public void Notify_HarbingerTreeDied()
	{
		nextHarbingerTreeCheckTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + 480000f + HarbingerTreeCheckIntervalHours.RandomInRange * 2500f);
	}

	public override void AppendDebugString(StringBuilder sb)
	{
		sb.AppendLine("  " + GetType().Name + ":");
		sb.AppendLine("    level:  " + level);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			ElevateLooseCorpseTrackers();
			foreach (var (key, _) in hypnotisedPawns.Where((KeyValuePair<Pawn, int> kvp) => kvp.Key.DestroyedOrNull() || kvp.Key.Dead).ToList())
			{
				hypnotisedPawns.Remove(key);
			}
		}
		Scribe_Values.Look(ref level, "level", 0);
		Scribe_Values.Look(ref highestLevelReached, "highestLevelReached", 0);
		Scribe_Defs.Look(ref levelDef, "levelDef");
		Scribe_References.Look(ref monolith, "monolith");
		Scribe_Values.Look(ref nextHarbingerTreeCheckTick, "nextHarbingerTreeCheckTick", 0);
		Scribe_Values.Look(ref lastLevelChangeTick, "lastLevelChangeTick", 0);
		Scribe_Values.Look(ref hasSeenGrayFlesh, "hasSeenGrayFlesh", defaultValue: false);
		Scribe_Values.Look(ref hasPerformedVoidProvocation, "hasPerformedVoidProvocation", defaultValue: false);
		Scribe_Values.Look(ref lastLevelActivationLetterSent, "lastLevelActivationLetterSent", -1);
		Scribe_References.Look(ref voidNodeActivator, "voidNodeActivator");
		Scribe_Collections.Look(ref metalHellPawns, "metalHellPawns", LookMode.Deep);
		Scribe_Values.Look(ref metalHellReturnTick, "metalHellReturnTick", 0);
		Scribe_Values.Look(ref metalHellReturnLetterText, "metalHellReturnLetterText");
		Scribe_Values.Look(ref fireGrayPallTick, "fireGrayPallTick", 0);
		Scribe_Values.Look(ref newestMetalhorrorBiosignatureTick, "newestMetalhorrorBiosignatureTick", 0);
		Scribe_Values.Look(ref hasBuiltHoldingPlatform, "hasBuiltHoldingPlatform", defaultValue: false);
		Scribe_Collections.Look(ref corpseTrackers, "corpses", LookMode.Reference, LookMode.Deep, ref workingPawnList, ref workingTrackerList);
		Scribe_Collections.Look(ref looseCorpseTrackers, "looseCorpseTrackers", LookMode.Deep);
		Scribe_Collections.Look(ref hypnotisedPawns, "hypnotisedPawns", LookMode.Reference, LookMode.Value, ref workingHypnotizedList, ref workingHypnotizedTickList);
		Scribe_Collections.Look(ref emergedBiosignatures, "emergedBiosignatures", LookMode.Value);
		Scribe_Collections.Look(ref monolithLetters, "monolithLetters", LookMode.Deep);
		Scribe_Values.Look(ref monolithStudyProgress, "monolithStudyProgress", 0);
		Scribe_Values.Look(ref monolithNextIndex, "monolithNextIndex", 0);
		Scribe_Values.Look(ref monolithAnomalyKnowledge, "monolithAnomalyKnowledge", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && looseCorpseTrackers == null)
		{
			looseCorpseTrackers = new List<UnnaturalCorpseTracker>();
		}
	}

	private void ElevateLooseCorpseTrackers()
	{
		foreach (var (pawn2, _) in corpseTrackers)
		{
			if (pawn2.Dead || pawn2.DestroyedOrNull())
			{
				toRemove.Add(pawn2);
			}
		}
		foreach (Pawn item in toRemove)
		{
			looseCorpseTrackers.Add(corpseTrackers[item]);
			corpseTrackers.Remove(item);
		}
		toRemove.Clear();
	}
}
