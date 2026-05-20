using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse.AI;

public class Pawn_MindState : IExposable
{
	public Pawn pawn;

	public MentalStateHandler mentalStateHandler;

	public MentalBreaker mentalBreaker;

	public MentalFitGenerator mentalFitGenerator;

	public InspirationHandler inspirationHandler;

	public PriorityWork priorityWork;

	private bool activeInt = true;

	public JobTag lastJobTag;

	public int lastIngestTick = -99999;

	public int nextApparelOptimizeTick = -99999;

	public bool canFleeIndividual = true;

	public int exitMapAfterTick = -99999;

	public int lastDisturbanceTick = -99999;

	public int hibernationEndedTick = -99999;

	public IntVec3 forcedGotoPosition = IntVec3.Invalid;

	public Thing knownExploder;

	public bool wantsToTradeWithColony;

	public bool traderDismissed;

	public bool hasQuest;

	public Thing lastMannedThing;

	public int canLovinTick = -99999;

	public int canSleepTick = -99999;

	public Pawn meleeThreat;

	public int lastMeleeThreatHarmTick = -99999;

	public int lastEngageTargetTick = -99999;

	public int lastAttackTargetTick = -99999;

	public LocalTargetInfo lastAttackedTarget;

	public Thing enemyTarget;

	public BreachingTargetData breachingTarget;

	public PawnDuty duty;

	public Dictionary<int, int> thinkData = new Dictionary<int, int>();

	public int lastAssignedInteractTime = -99999;

	public int interactionsToday;

	public int lastDayInteractionTick;

	public int lastInventoryRawFoodUseTick;

	public bool nextMoveOrderIsWait;

	public bool nextMoveOrderIsCrawlBreak;

	public int lastTakeCombatEnhancingDrugTick = -99999;

	public int lastTakeRecreationalDrugTick = -60000;

	public int lastHarmTick = -99999;

	public int noAidRelationsGainUntilTick = -99999;

	public bool anyCloseHostilesRecently;

	public int applyBedThoughtsTick;

	public int applyThroneThoughtsTick;

	public bool applyBedThoughtsOnLeave;

	public bool willJoinColonyIfRescuedInt;

	private bool wildManEverReachedOutsideInt;

	public int timesGuestTendedToByPlayer;

	public int lastSelfTendTick = -99999;

	public bool spawnedByInfestationThingComp;

	public int lastPredatorHuntingPlayerNotificationTick = -99999;

	public int lastSlaveSuppressedTick = -99999;

	public ThingDef lastBedDefSleptIn;

	public int lastHumanMeatIngestedTick = -99999;

	public int? lastStartRoamCooldownTick;

	public int nextInventoryStockTick = -99999;

	public bool returnToHealingPod;

	private Dictionary<Pawn, AutofeedMode> autoFeeders = new Dictionary<Pawn, AutofeedMode>();

	private Dictionary<Pawn, float> babyCaravanBreastfeed = new Dictionary<Pawn, float>();

	public int nextSleepingBreastfeedStrictTick = -99999;

	public ResurrectCorpseData resurrectTarget;

	public int lastRangedHarmTick;

	public Thing droppedWeapon;

	public int lastRotStinkTick = -99999;

	public int lastBroughtToSafeTemperatureTick = -99999;

	public int lastBecameVisibleTick = -99999;

	public int lastBecameInvisibleTick = -99999;

	public int lastForcedVisibleTick = -99999;

	public int lastCombatantTick = -99999;

	public int entityTicksInCaptivity;

	public int lastSwamTick = -99999;

	public float maxDistToSquadFlag = -1f;

	private List<Pawn> tmpLoadPawns;

	private List<AutofeedMode> tmpLoadAutofeedMode;

	private List<float> tmpLoadFloat;

	private const int UpdateAnyCloseHostilesRecentlyEveryTicks = 100;

	private const int AnyCloseHostilesRecentlyRegionsToScan_ToActivate = 18;

	private const int AnyCloseHostilesRecentlyRegionsToScan_ToDeactivate = 24;

	private const float HarmForgetDistance = 3f;

	private const int MeleeHarmForgetDelay = 400;

	private const float ClamorImpactFleeChance = 0.4f;

	private const float PawnActionFleeOffMapChance = 0.5f;

	private const int RoamingCooldownTicks = 30000;

	private static readonly IntRange LastHumanMeatEatenTicksRange = new IntRange(0, 60000);

	public static readonly IntRange NextSleepingBreastfeedTickCheck = new IntRange(300, 500);

	public const int DefaultCombatantCooldown = 3600;

	public const int WeatherThoughInterval = 120;

	public const int SwimSoakingWetImmuneDuration = 7500;

	public bool InRoamingCooldown
	{
		get
		{
			if (lastStartRoamCooldownTick.HasValue)
			{
				return lastStartRoamCooldownTick.Value + 30000 > Find.TickManager.TicksGame;
			}
			return false;
		}
	}

	public bool AvailableForGoodwillReward => Find.TickManager.TicksGame >= noAidRelationsGainUntilTick;

	public bool Active
	{
		get
		{
			return activeInt;
		}
		set
		{
			if (value != activeInt)
			{
				activeInt = value;
				if (pawn.Spawned)
				{
					pawn.Map.mapPawns.UpdateRegistryForPawn(pawn);
				}
			}
		}
	}

	public bool IsIdle
	{
		get
		{
			if (pawn.Downed)
			{
				return false;
			}
			if (!pawn.Spawned)
			{
				return false;
			}
			return lastJobTag == JobTag.Idle;
		}
	}

	public bool MeleeThreatStillThreat
	{
		get
		{
			if (meleeThreat != null && meleeThreat.Spawned && !meleeThreat.ThreatDisabled(pawn) && meleeThreat.Awake() && pawn.Spawned && Find.TickManager.TicksGame <= lastMeleeThreatHarmTick + 400 && (float)(pawn.Position - meleeThreat.Position).LengthHorizontalSquared <= 9f)
			{
				return GenSight.LineOfSight(pawn.Position, meleeThreat.Position, pawn.Map);
			}
			return false;
		}
	}

	public bool WildManEverReachedOutside
	{
		get
		{
			return wildManEverReachedOutsideInt;
		}
		set
		{
			if (wildManEverReachedOutsideInt != value)
			{
				wildManEverReachedOutsideInt = value;
				ReachabilityUtility.ClearCacheFor(pawn);
			}
		}
	}

	public bool WillJoinColonyIfRescued
	{
		get
		{
			return willJoinColonyIfRescuedInt;
		}
		set
		{
			if (willJoinColonyIfRescuedInt != value)
			{
				willJoinColonyIfRescuedInt = value;
				if (pawn.Spawned)
				{
					pawn.Map.attackTargetsCache.UpdateTarget(pawn);
				}
			}
		}
	}

	public bool AnythingPreventsJoiningColonyIfRescued
	{
		get
		{
			if (pawn.Faction == Faction.OfPlayer)
			{
				return true;
			}
			if (pawn.IsPrisoner && !pawn.HostFaction.HostileTo(Faction.OfPlayer))
			{
				return true;
			}
			if (!pawn.IsPrisoner && pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer) && !pawn.Downed)
			{
				return true;
			}
			return false;
		}
	}

	public bool CombatantRecently
	{
		get
		{
			if (lastCombatantTick >= 0)
			{
				return Find.TickManager.TicksGame <= lastCombatantTick + 3600;
			}
			return false;
		}
	}

	public bool WasRecentlyCombatantTicks(int ticksDuration)
	{
		if (lastCombatantTick >= 0)
		{
			return Find.TickManager.TicksGame <= lastCombatantTick + ticksDuration;
		}
		return false;
	}

	public Pawn_MindState()
	{
	}

	public Pawn_MindState(Pawn pawn)
	{
		this.pawn = pawn;
		mentalStateHandler = new MentalStateHandler(pawn);
		mentalBreaker = new MentalBreaker(pawn);
		mentalFitGenerator = new MentalFitGenerator(pawn);
		inspirationHandler = new InspirationHandler(pawn);
		priorityWork = new PriorityWork(pawn);
	}

	public void Reset(bool clearInspiration = false, bool clearMentalState = true)
	{
		Reset(clearInspiration, clearMentalState, wasDowned: false);
	}

	public void Reset(bool clearInspiration = false, bool clearMentalState = true, bool wasDowned = false)
	{
		if (clearMentalState)
		{
			mentalStateHandler.Reset();
			mentalBreaker.Reset();
			mentalFitGenerator.Reset();
		}
		if (clearInspiration)
		{
			inspirationHandler.Reset();
		}
		activeInt = true;
		lastJobTag = JobTag.Misc;
		lastIngestTick = -99999;
		nextApparelOptimizeTick = -99999;
		canFleeIndividual = true;
		exitMapAfterTick = -99999;
		lastDisturbanceTick = -99999;
		forcedGotoPosition = IntVec3.Invalid;
		knownExploder = null;
		wantsToTradeWithColony = false;
		traderDismissed = false;
		hasQuest = false;
		lastMannedThing = null;
		canLovinTick = -99999;
		canSleepTick = -99999;
		meleeThreat = null;
		lastMeleeThreatHarmTick = -99999;
		lastEngageTargetTick = -99999;
		lastAttackTargetTick = -99999;
		lastAttackedTarget = LocalTargetInfo.Invalid;
		enemyTarget = null;
		breachingTarget = null;
		duty = null;
		thinkData.Clear();
		lastAssignedInteractTime = -99999;
		interactionsToday = 0;
		lastInventoryRawFoodUseTick = 0;
		priorityWork.Clear();
		nextMoveOrderIsWait = true;
		lastTakeCombatEnhancingDrugTick = -99999;
		lastHarmTick = -99999;
		lastRangedHarmTick = -99999;
		anyCloseHostilesRecently = false;
		WillJoinColonyIfRescued = false;
		WildManEverReachedOutside = false;
		lastSelfTendTick = -99999;
		spawnedByInfestationThingComp = false;
		lastPredatorHuntingPlayerNotificationTick = -99999;
		lastSlaveSuppressedTick = -99999;
		lastStartRoamCooldownTick = null;
		nextInventoryStockTick = -99999;
		returnToHealingPod = false;
		resurrectTarget = null;
		lastRotStinkTick = -99999;
		lastSwamTick = -99999;
		if (!wasDowned)
		{
			timesGuestTendedToByPlayer = 0;
		}
	}

	public void Notify_PawnRedressed()
	{
		lastTakeRecreationalDrugTick = Find.TickManager.TicksGame - 60000;
		SetupLastHumanMeatTick();
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref meleeThreat, "meleeThreat");
		Scribe_References.Look(ref enemyTarget, "enemyTarget");
		Scribe_References.Look(ref knownExploder, "knownExploder");
		Scribe_References.Look(ref lastMannedThing, "lastMannedThing");
		Scribe_References.Look(ref droppedWeapon, "droppedWeapon");
		Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
		Scribe_Collections.Look(ref thinkData, "thinkData", LookMode.Value, LookMode.Value);
		Scribe_Values.Look(ref activeInt, "active", defaultValue: true);
		Scribe_Values.Look(ref lastJobTag, "lastJobTag", JobTag.Misc);
		Scribe_Values.Look(ref lastIngestTick, "lastIngestTick", -99999);
		Scribe_Values.Look(ref nextApparelOptimizeTick, "nextApparelOptimizeTick", -99999);
		Scribe_Values.Look(ref lastEngageTargetTick, "lastEngageTargetTick", 0);
		Scribe_Values.Look(ref lastAttackTargetTick, "lastAttackTargetTick", 0);
		Scribe_Values.Look(ref canFleeIndividual, "canFleeIndividual", defaultValue: false);
		Scribe_Values.Look(ref exitMapAfterTick, "exitMapAfterTick", -99999);
		Scribe_Values.Look(ref forcedGotoPosition, "forcedGotoPosition", IntVec3.Invalid);
		Scribe_Values.Look(ref lastMeleeThreatHarmTick, "lastMeleeThreatHarmTick", 0);
		Scribe_Values.Look(ref lastAssignedInteractTime, "lastAssignedInteractTime", -99999);
		Scribe_Values.Look(ref interactionsToday, "interactionsToday", 0);
		Scribe_Values.Look(ref lastInventoryRawFoodUseTick, "lastInventoryRawFoodUseTick", 0);
		Scribe_Values.Look(ref lastDisturbanceTick, "lastDisturbanceTick", -99999);
		Scribe_Values.Look(ref wantsToTradeWithColony, "wantsToTradeWithColony", defaultValue: false);
		Scribe_Values.Look(ref hasQuest, "hasQuest", defaultValue: false);
		Scribe_Values.Look(ref canLovinTick, "canLovinTick", -99999);
		Scribe_Values.Look(ref canSleepTick, "canSleepTick", -99999);
		Scribe_Values.Look(ref nextMoveOrderIsWait, "nextMoveOrderIsWait", defaultValue: true);
		Scribe_Values.Look(ref lastTakeCombatEnhancingDrugTick, "lastTakeCombatEnhancingDrugTick", -99999);
		Scribe_Values.Look(ref lastTakeRecreationalDrugTick, "lastTakeRecreationalDrugTick", -60000);
		Scribe_Values.Look(ref lastHarmTick, "lastHarmTick", -99999);
		Scribe_Values.Look(ref anyCloseHostilesRecently, "anyCloseHostilesRecently", defaultValue: false);
		Scribe_Deep.Look(ref duty, "duty");
		Scribe_Deep.Look(ref mentalStateHandler, "mentalStateHandler", pawn);
		Scribe_Deep.Look(ref mentalBreaker, "mentalBreaker", pawn);
		Scribe_Deep.Look(ref mentalFitGenerator, "mentalFitGenerator", pawn);
		Scribe_Deep.Look(ref inspirationHandler, "inspirationHandler", pawn);
		Scribe_Deep.Look(ref priorityWork, "priorityWork", pawn);
		Scribe_Values.Look(ref applyBedThoughtsTick, "applyBedThoughtsTick", 0);
		Scribe_Values.Look(ref applyThroneThoughtsTick, "applyThroneThoughtsTick", 0);
		Scribe_Values.Look(ref applyBedThoughtsOnLeave, "applyBedThoughtsOnLeave", defaultValue: false);
		Scribe_Values.Look(ref willJoinColonyIfRescuedInt, "willJoinColonyIfRescued", defaultValue: false);
		Scribe_Values.Look(ref wildManEverReachedOutsideInt, "wildManEverReachedOutside", defaultValue: false);
		Scribe_Values.Look(ref timesGuestTendedToByPlayer, "timesGuestTendedToByPlayer", 0);
		Scribe_Values.Look(ref noAidRelationsGainUntilTick, "noAidRelationsGainUntilTick", -99999);
		Scribe_Values.Look(ref lastSelfTendTick, "lastSelfTendTick", 0);
		Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
		Scribe_Values.Look(ref lastPredatorHuntingPlayerNotificationTick, "lastPredatorHuntingPlayerNotificationTick", -99999);
		Scribe_Deep.Look(ref breachingTarget, "breachingTarget");
		Scribe_Values.Look(ref lastSlaveSuppressedTick, "lastSlaveSuppressedTick", -99999);
		Scribe_Values.Look(ref lastHumanMeatIngestedTick, "lastHumanMeatIngestedTick", -99999);
		Scribe_Values.Look(ref lastStartRoamCooldownTick, "lastStartRoamCooldownTick");
		Scribe_Values.Look(ref nextInventoryStockTick, "nextInventoryStockTick", -99999);
		Scribe_Values.Look(ref returnToHealingPod, "returnToHealingPod", defaultValue: false);
		Scribe_Defs.Look(ref lastBedDefSleptIn, "lastBedDefSleptIn");
		Scribe_Collections.Look(ref autoFeeders, "babyAutoBreastfeedMoms", LookMode.Reference, LookMode.Value, ref tmpLoadPawns, ref tmpLoadAutofeedMode, logNullErrors: false);
		Scribe_Collections.Look(ref babyCaravanBreastfeed, "babyCaravanBreastfeed", LookMode.Reference, LookMode.Value, ref tmpLoadPawns, ref tmpLoadFloat, logNullErrors: false);
		Scribe_Deep.Look(ref resurrectTarget, "resurrectTarget", null);
		Scribe_Values.Look(ref lastRangedHarmTick, "lastRangedHarmTick", -99999);
		Scribe_Values.Look(ref lastRotStinkTick, "lastRotStinkTick", -99999);
		Scribe_Values.Look(ref lastBroughtToSafeTemperatureTick, "lastBroughtToSafeTemperatureTick", -99999);
		Scribe_Values.Look(ref lastBecameVisibleTick, "lastBecameVisibleTick", -99999);
		Scribe_Values.Look(ref lastBecameInvisibleTick, "lastBecameInvisibleTick", -99999);
		Scribe_Values.Look(ref lastForcedVisibleTick, "lastForcedVisibleTick", -99999);
		Scribe_Values.Look(ref lastCombatantTick, "lastCombatantTick", -99999);
		Scribe_Values.Look(ref entityTicksInCaptivity, "entityTicksInCaptivity", 0);
		Scribe_Values.Look(ref hibernationEndedTick, "hibernationEndedTick", -99999);
		Scribe_Values.Look(ref lastDayInteractionTick, "lastDayInteractionTick", 0);
		Scribe_Values.Look(ref lastSwamTick, "lastSwamTick", -99999);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && autoFeeders == null)
		{
			autoFeeders = new Dictionary<Pawn, AutofeedMode>();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && babyCaravanBreastfeed == null)
		{
			babyCaravanBreastfeed = new Dictionary<Pawn, float>();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && mentalFitGenerator == null)
		{
			mentalFitGenerator = new MentalFitGenerator(pawn);
		}
		BackCompatibility.PostExposeData(this);
	}

	public void MindStateTickInterval(int delta)
	{
		if (wantsToTradeWithColony)
		{
			TradeUtility.CheckInteractWithTradersTeachOpportunity(this.pawn);
		}
		if (meleeThreat != null && !MeleeThreatStillThreat)
		{
			meleeThreat = null;
		}
		mentalStateHandler.MentalStateHandlerTickInterval(delta);
		mentalBreaker.MentalBreakerTickInterval(delta);
		mentalFitGenerator.TickInterval(delta);
		inspirationHandler.InspirationHandlerTickInterval(delta);
		if (!this.pawn.GetPosture().Laying())
		{
			applyBedThoughtsTick = 0;
		}
		if (this.pawn.IsHashIntervalTick(100, delta))
		{
			if (this.pawn.Spawned)
			{
				int regionsToScan = (anyCloseHostilesRecently ? 24 : 18);
				anyCloseHostilesRecently = PawnUtility.EnemiesAreNearby(this.pawn, regionsToScan, passDoors: true);
			}
			else
			{
				anyCloseHostilesRecently = false;
			}
		}
		if (WillJoinColonyIfRescued && AnythingPreventsJoiningColonyIfRescued)
		{
			WillJoinColonyIfRescued = false;
		}
		if (this.pawn.Spawned && this.pawn.IsWildMan() && !WildManEverReachedOutside && this.pawn.GetDistrict() != null && this.pawn.GetDistrict().TouchesMapEdge)
		{
			WildManEverReachedOutside = true;
		}
		if (this.pawn.Spawned && this.pawn.RaceProps.IsFlesh && this.pawn.needs.mood != null && this.pawn.IsHashIntervalTick(120, delta))
		{
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.Map);
			if (CanGainGainThoughtNow(terrain.traversedThought))
			{
				this.pawn.needs.mood.thoughts.memories.TryGainMemoryFast(terrain.traversedThought);
			}
			WeatherDef curWeatherLerped = this.pawn.Map.weatherManager.CurWeatherLerped;
			if (CanGainGainThoughtNow(curWeatherLerped.weatherThought))
			{
				bool flag = this.pawn.Position.Roofed(this.pawn.Map);
				bool flag2 = curWeatherLerped.weatherThought.stages.Count == 1;
				if (!flag || !flag2)
				{
					int stage = ((!(flag2 || flag)) ? 1 : 0);
					this.pawn.needs.mood.thoughts.memories.TryGainMemoryFast(curWeatherLerped.weatherThought, stage);
				}
			}
			if (this.pawn.Position.GasDensity(this.pawn.Map, GasType.RotStink) > 0 && GasUtility.IsAffectedByExposure(this.pawn))
			{
				lastRotStinkTick = Find.TickManager.TicksGame;
			}
		}
		if (droppedWeapon != null && !droppedWeapon.Spawned)
		{
			droppedWeapon = null;
		}
		int num = GenLocalDate.DayTick(this.pawn);
		if (num < lastDayInteractionTick)
		{
			interactionsToday = 0;
		}
		lastDayInteractionTick = num;
		if ((this.pawn.IsFighting() && this.pawn.CurJob?.def != JobDefOf.Wait_Combat) || this.pawn.equipment?.Primary != null)
		{
			lastCombatantTick = Find.TickManager.TicksGame;
		}
		if (enemyTarget is Pawn { mindState: not null } pawn)
		{
			pawn.mindState.lastCombatantTick = Find.TickManager.TicksGame;
		}
	}

	private bool CanGainGainThoughtNow(ThoughtDef thought)
	{
		if (thought == null)
		{
			return false;
		}
		if (thought == ThoughtDefOf.SoakingWet && GenTicks.TicksGame - lastSwamTick < 7500)
		{
			return false;
		}
		return true;
	}

	public void JoinColonyBecauseRescuedBy(Pawn by)
	{
		WillJoinColonyIfRescued = false;
		if (!AnythingPreventsJoiningColonyIfRescued)
		{
			InteractionWorker_RecruitAttempt.DoRecruit(by, pawn, useAudiovisualEffects: false);
			if (pawn.needs != null && pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Rescued);
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedMeByOfferingHelp, by);
			}
			TaggedString text = "LetterRescueQuestFinished".Translate(pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst();
			if (!pawn.Map.wasSpawnedViaGravShipLanding)
			{
				text += " " + "LetterRescueQuestFinishedCaravanExtra".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelRescueQuestFinished".Translate(), text, LetterDefOf.PositiveEvent, pawn);
		}
	}

	public void ResetLastDisturbanceTick()
	{
		lastDisturbanceTick = -9999999;
	}

	public void SetupLastHumanMeatTick()
	{
		if (pawn.Ideo != null && pawn.Ideo.IdeoCausesHumanMeatCravings())
		{
			lastHumanMeatIngestedTick = Find.TickManager.TicksGame;
			lastHumanMeatIngestedTick -= LastHumanMeatEatenTicksRange.RandomInRange;
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (pawn.IsColonistPlayerControlled || pawn.IsColonySubhumanPlayerControlled)
		{
			foreach (Gizmo gizmo in priorityWork.GetGizmos())
			{
				yield return gizmo;
			}
		}
		foreach (Gizmo gizmo2 in CaravanFormingUtility.GetGizmos(pawn))
		{
			yield return gizmo2;
		}
	}

	public void SetNoAidRelationsGainUntilTick(int tick)
	{
		if (tick > noAidRelationsGainUntilTick)
		{
			noAidRelationsGainUntilTick = tick;
		}
	}

	public void Notify_OutfitChanged()
	{
		nextApparelOptimizeTick = Find.TickManager.TicksGame;
	}

	public void Notify_DamageTaken(DamageInfo dinfo)
	{
		mentalStateHandler.Notify_DamageTaken(dinfo);
		if (!dinfo.Def.ExternalViolenceFor(this.pawn))
		{
			return;
		}
		lastHarmTick = Find.TickManager.TicksGame;
		if (dinfo.Def.isRanged)
		{
			lastRangedHarmTick = Find.TickManager.TicksGame;
		}
		if (this.pawn.Spawned)
		{
			Pawn pawn = dinfo.Instigator as Pawn;
			if (!mentalStateHandler.InMentalState && dinfo.Instigator != null && (pawn != null || dinfo.Instigator is Building_Turret) && dinfo.Instigator.Faction != null && (!ModsConfig.AnomalyActive || dinfo.Instigator.Faction != Faction.OfEntities) && (dinfo.Instigator.Faction.def.humanlikeFaction || (pawn != null && (int)pawn.def.race.intelligence >= 1)) && this.pawn.Faction == null && (this.pawn.IsAnimal || this.pawn.IsWildMan()) && (this.pawn.CurJob == null || this.pawn.CurJob.def != JobDefOf.PredatorHunt || dinfo.Instigator != ((JobDriver_PredatorHunt)this.pawn.jobs.curDriver).Prey) && Rand.Chance(PawnUtility.GetManhunterOnDamageChance(this.pawn, dinfo.Instigator)))
			{
				StartManhunterBecauseOfPawnAction(pawn, "AnimalManhunterFromDamage", causedByDamage: true);
			}
			else if (dinfo.Instigator != null && dinfo.Def.makesAnimalsFlee && FleeUtility.ShouldAnimalFleeDanger(this.pawn))
			{
				StartFleeingBecauseOfPawnAction(dinfo.Instigator);
			}
		}
		if (this.pawn.GetPosture() != PawnPosture.Standing)
		{
			lastDisturbanceTick = Find.TickManager.TicksGame;
		}
	}

	public void Notify_ClamorImpact(Thing instigator)
	{
		canSleepTick = Find.TickManager.TicksGame + 1000;
		if (pawn.IsAnimal && instigator is Projectile && ((Projectile)instigator).AnimalsFleeImpact && (pawn.playerSettings == null || pawn.playerSettings.Master == null) && Rand.Chance(0.4f) && FleeUtility.ShouldAnimalFleeDanger(pawn))
		{
			StartFleeingBecauseOfPawnAction(((Projectile)instigator).Launcher);
		}
	}

	internal void Notify_EngagedTarget()
	{
		lastEngageTargetTick = Find.TickManager.TicksGame;
	}

	internal void Notify_AttackedTarget(LocalTargetInfo target)
	{
		lastAttackTargetTick = Find.TickManager.TicksGame;
		lastAttackedTarget = target;
	}

	internal bool CheckStartMentalStateBecauseRecruitAttempted(Pawn tamer)
	{
		if (!pawn.IsAnimal && (!pawn.IsWildMan() || pawn.IsPrisoner))
		{
			return false;
		}
		if (!mentalStateHandler.InMentalState && pawn.Faction == null && Rand.Chance(PawnUtility.GetManhunterOnTameFailChance(pawn)))
		{
			StartManhunterBecauseOfPawnAction(tamer, "AnimalManhunterFromTaming");
			return true;
		}
		return false;
	}

	internal void Notify_DangerousExploderAboutToExplode(Thing exploder)
	{
		if ((int)pawn.RaceProps.intelligence >= 2 && !pawn.Drafted)
		{
			knownExploder = exploder;
			pawn.jobs.CheckForJobOverride();
		}
	}

	public void Notify_Explosion(Explosion explosion)
	{
		if (pawn.Faction == null && !(explosion.radius < 3.5f) && pawn.Position.InHorDistOf(explosion.Position, explosion.radius + 7f) && FleeUtility.ShouldAnimalFleeDanger(pawn))
		{
			StartFleeingBecauseOfPawnAction(explosion);
		}
	}

	public void Notify_TuckedIntoBed()
	{
		if (pawn.IsWildMan())
		{
			WildManEverReachedOutside = false;
		}
		ResetLastDisturbanceTick();
	}

	public void Notify_SelfTended()
	{
		lastSelfTendTick = Find.TickManager.TicksGame;
	}

	public void Notify_PredatorHuntingPlayerNotification()
	{
		lastPredatorHuntingPlayerNotificationTick = Find.TickManager.TicksGame;
	}

	private IEnumerable<Pawn> GetPackmates(Pawn pawn, float radius)
	{
		if (!pawn.Spawned)
		{
			yield break;
		}
		District pawnRoom = pawn.GetDistrict();
		IReadOnlyList<Pawn> raceMates = pawn.Map.mapPawns.AllPawnsSpawned;
		HashSet<ThingDef> acceptableDefs = new HashSet<ThingDef>(pawn.RaceProps.crossAggroWith.OrElseEmptyEnumerable().Prepend(pawn.def));
		for (int i = 0; i < raceMates.Count; i++)
		{
			if (pawn != raceMates[i] && acceptableDefs.Contains(raceMates[i].def) && raceMates[i].Faction == pawn.Faction && raceMates[i].Position.InHorDistOf(pawn.Position, radius) && raceMates[i].GetDistrict() == pawnRoom)
			{
				yield return raceMates[i];
			}
		}
	}

	private void StartManhunterBecauseOfPawnAction(Pawn instigator, string letterTextKey, bool causedByDamage = false)
	{
		if (!mentalStateHandler.TryStartMentalState(PawnUtility.ManhunterStateFor(this.pawn)))
		{
			return;
		}
		string text = letterTextKey.Translate(this.pawn.Label, this.pawn.Named("PAWN")).AdjustedFor(this.pawn);
		GlobalTargetInfo globalTargetInfo = this.pawn;
		float num = 0.5f;
		if (causedByDamage)
		{
			num *= PawnUtility.GetManhunterChanceFactorForInstigator(instigator);
		}
		int num2 = 1;
		if (Find.Storyteller.difficulty.allowBigThreats && Rand.Value < num)
		{
			PawnKindDef pawnKindDef = this.pawn.RaceProps.manhunterPackUseLabelFrom ?? this.pawn.kindDef;
			Pawn pawn = this.pawn;
			foreach (Pawn packmate in GetPackmates(this.pawn, 24f))
			{
				if (packmate.mindState.mentalStateHandler.TryStartMentalState(PawnUtility.ManhunterStateFor(packmate), null, forced: false, forceWake: false, causedByMood: false, null, transitionSilently: false, causedByDamage))
				{
					num2++;
					if (packmate.kindDef == pawnKindDef)
					{
						pawn = packmate;
					}
				}
			}
			if (num2 > 1)
			{
				globalTargetInfo = new TargetInfo(this.pawn.Position, this.pawn.Map);
				text += "\n\n";
				text += "AnimalManhunterOthers".Translate(pawnKindDef.GetLabelPlural(), pawn);
			}
		}
		string text2 = (this.pawn.IsAnimal ? this.pawn.Label : this.pawn.def.label);
		string text3 = "LetterLabelAnimalManhunterRevenge".Translate(text2).CapitalizeFirst();
		Find.LetterStack.ReceiveLetter(text3, text, (num2 == 1) ? LetterDefOf.ThreatSmall : LetterDefOf.ThreatBig, globalTargetInfo);
	}

	public void StartFleeingBecauseOfPawnAction(Thing instigator)
	{
		if (pawn.RaceProps.canLeaveMapFlying && pawn.Faction != Faction.OfPlayer && !pawn.Position.Roofed(pawn.Map) && !pawn.IsQuestLodger() && pawn.flight.CanEverFly && Rand.Chance(pawn.RaceProps.leaveMapOnFleeChance))
		{
			pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.ExitMapFlying), JobCondition.InterruptForced);
			return;
		}
		List<Thing> threats = new List<Thing> { instigator };
		IntVec3 fleeDest = CellFinderLoose.GetFleeDest(pawn, threats, pawn.Position.DistanceTo(instigator.Position) + 28f);
		if (fleeDest != pawn.Position)
		{
			Vector3 lhs = (fleeDest - pawn.Position).ToVector3();
			Vector3 rhs = (pawn.Map.Center - pawn.Position).ToVector3();
			bool flag = Vector3.Dot(lhs, rhs) < 0f;
			if (pawn.IsAnimal && pawn.Faction == null && flag && Rand.Chance(0.5f) && CellFinderLoose.GetFleeExitPosition(pawn, 10f, out var position))
			{
				Job job = JobMaker.MakeJob(JobDefOf.Flee, position, instigator);
				job.exitMapOnArrival = true;
				pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
			}
			else
			{
				pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Flee, fleeDest, instigator), JobCondition.InterruptOptional);
			}
		}
		if (!pawn.RaceProps.herdAnimal || !Rand.Chance(0.1f))
		{
			return;
		}
		foreach (Pawn packmate in GetPackmates(pawn, 24f))
		{
			if (FleeUtility.ShouldAnimalFleeDanger(packmate))
			{
				IntVec3 fleeDest2 = CellFinderLoose.GetFleeDest(packmate, threats, packmate.Position.DistanceTo(instigator.Position) + 28f);
				if (fleeDest2 != packmate.Position)
				{
					packmate.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Flee, fleeDest2, instigator), JobCondition.InterruptOptional);
				}
			}
		}
	}

	public void SetAutofeeder(Pawn feeder, AutofeedMode setting)
	{
		if (setting == AutofeedMode.Childcare)
		{
			autoFeeders.Remove(feeder);
		}
		else
		{
			autoFeeders[feeder] = setting;
		}
		if (setting == AutofeedMode.Childcare)
		{
			if (feeder.WorkTypeIsDisabled(WorkTypeDefOf.Childcare))
			{
				Messages.Message("MessageChildcareDisabled".Translate(feeder.Named("FEEDER")), feeder, MessageTypeDefOf.CautionInput, historical: false);
			}
			else if (!feeder.workSettings.WorkIsActive(WorkTypeDefOf.Childcare))
			{
				Messages.Message("MessageChildcareNotAssigned".Translate(feeder.Named("FEEDER")), feeder, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
	}

	public AutofeedMode AutofeedSetting(Pawn feeder)
	{
		if (!pawn.DevelopmentalStage.Baby())
		{
			return AutofeedMode.Never;
		}
		return autoFeeders.TryGetValue(feeder, AutofeedMode.Childcare);
	}

	public IEnumerable<Pawn> Autofeeders()
	{
		if (!ChildcareUtility.CanSuckle(pawn, out var _))
		{
			return Enumerable.Empty<Pawn>();
		}
		return autoFeeders.Keys;
	}

	public bool AnyAutofeeder(AutofeedMode autofeed, Predicate<Pawn, Pawn> feederPredicate, List<Pawn> possibleFeeders = null)
	{
		if (!ChildcareUtility.CanSuckle(pawn, out var _))
		{
			return false;
		}
		possibleFeeders = possibleFeeders ?? pawn.MapHeld.mapPawns.FreeHumanlikesOfFaction(pawn.Faction);
		foreach (Pawn possibleFeeder in possibleFeeders)
		{
			if (possibleFeeder != pawn && autofeed == AutofeedSetting(possibleFeeder) && feederPredicate(pawn, possibleFeeder))
			{
				return true;
			}
		}
		return false;
	}

	public void ClearBreastfeedCaravan()
	{
		babyCaravanBreastfeed.Clear();
	}

	public void BreastfeedCaravan(Pawn baby, float amountPct)
	{
		if (pawn.GetCaravan() == null)
		{
			Log.Warning(pawn.Label + " tried to caravan breastfeed while not in a caravan");
		}
		if (!babyCaravanBreastfeed.ContainsKey(baby))
		{
			babyCaravanBreastfeed.Add(baby, 0f);
		}
		float num = babyCaravanBreastfeed[baby] + amountPct;
		if (num >= 0.6f)
		{
			num -= 0.6f;
			baby.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BreastfedMe, pawn);
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BreastfedBaby, baby);
		}
		babyCaravanBreastfeed[baby] = num;
	}
}
