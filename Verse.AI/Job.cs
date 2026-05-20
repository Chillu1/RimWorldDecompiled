using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace Verse.AI;

public class Job : IExposable, ILoadReferenceable
{
	public JobDef def;

	public LocalTargetInfo targetA = LocalTargetInfo.Invalid;

	public LocalTargetInfo targetB = LocalTargetInfo.Invalid;

	public LocalTargetInfo targetC = LocalTargetInfo.Invalid;

	public List<LocalTargetInfo> targetQueueA;

	public List<LocalTargetInfo> targetQueueB;

	public GlobalTargetInfo globalTarget = GlobalTargetInfo.Invalid;

	public int count = -1;

	public List<int> countQueue;

	public int loadID = -1;

	public int startTick = -1;

	public int expiryInterval = -1;

	public bool checkOverrideOnExpire;

	public bool playerForced;

	public bool playerInterruptedForced;

	public bool showCarryingInspectLine = true;

	public bool flying;

	public bool swimming;

	public TargetIndex intervalScalingTarget;

	public List<ThingCountClass> placedThings;

	public int maxNumMeleeAttacks = int.MaxValue;

	public int maxNumStaticAttacks = int.MaxValue;

	public LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;

	public HaulMode haulMode;

	public Bill bill;

	public ICommunicable commTarget;

	public ThingDef plantDefToSow;

	public ThingDef thingDefToCarry;

	public Verb verbToUse;

	public bool haulOpportunisticDuplicates;

	public bool exitMapOnArrival;

	public bool failIfCantJoinOrCreateCaravan;

	public bool killIncappedTarget;

	public bool ignoreForbidden;

	public bool ignoreDesignations;

	public bool canBashDoors;

	public bool canBashFences;

	public bool canUseRangedWeapon = true;

	public bool haulDroppedApparel;

	public bool restUntilHealed;

	public bool ignoreJoyTimeAssignment;

	public bool doUntilGatheringEnded;

	public bool overeat;

	public bool ingestTotalCount;

	public bool attackDoorIfTargetLost;

	public int takeExtraIngestibles;

	public bool expireRequiresEnemiesNearby;

	public bool ensureReachable;

	public bool expireOnEnemiesNearby;

	public Lord lord;

	public bool collideWithPawns;

	public bool forceSleep;

	public InteractionDef interaction;

	public bool endIfCantShootTargetFromCurPos;

	public bool endIfCantShootInMelee;

	public bool checkEncumbrance;

	public float followRadius;

	public bool endAfterTendedOnce;

	public Quest quest;

	public Mote mote;

	public float psyfocusTargetLast = -1f;

	public bool wasOnMeditationTimeAssignment;

	public bool reactingToMeleeThreat;

	public bool preventFriendlyFire;

	public RopingPriority ropingPriority;

	public bool ropeToUnenclosedPens;

	public bool showSpeechBubbles = true;

	public Direction8Way lookDirection = Direction8Way.Invalid;

	public Rot4 overrideFacing = Rot4.Invalid;

	public bool forceMaintainFacing;

	public string dutyTag;

	public string ritualTag;

	public string controlGroupTag;

	public int takeInventoryDelay;

	public bool draftedTend;

	public bool speechFaceSpectatorsIfPossible;

	public SoundDef speechSoundMale;

	public SoundDef speechSoundFemale;

	public string biosculpterCycleKey;

	[MustTranslate]
	public string reportStringOverride;

	[MustTranslate]
	public string crawlingReportStringOverride;

	public bool startInvoluntarySleep;

	public bool isLearningDesire;

	public int interactableIndex = -1;

	public ThinkTreeDef jobGiverThinkTree;

	public ThinkNode jobGiver;

	public WorkGiverDef workGiverDef;

	public Ability ability;

	public ILoadReferenceable source;

	private JobDriver cachedDriver;

	private JobDriver lastJobDriverMade;

	private int jobGiverKey = -1;

	public RecipeDef RecipeDef => bill?.recipe;

	public JobDriver GetCachedDriverDirect => cachedDriver;

	public void Clear()
	{
		def = null;
		targetA = LocalTargetInfo.Invalid;
		targetB = LocalTargetInfo.Invalid;
		targetC = LocalTargetInfo.Invalid;
		targetQueueA = null;
		targetQueueB = null;
		count = -1;
		countQueue = null;
		loadID = -1;
		startTick = -1;
		expiryInterval = -1;
		checkOverrideOnExpire = false;
		playerForced = false;
		playerInterruptedForced = false;
		placedThings = null;
		maxNumMeleeAttacks = int.MaxValue;
		maxNumStaticAttacks = int.MaxValue;
		locomotionUrgency = LocomotionUrgency.Jog;
		haulMode = HaulMode.Undefined;
		bill = null;
		commTarget = null;
		plantDefToSow = null;
		verbToUse = null;
		haulOpportunisticDuplicates = false;
		exitMapOnArrival = false;
		failIfCantJoinOrCreateCaravan = false;
		killIncappedTarget = false;
		ignoreForbidden = false;
		ignoreDesignations = false;
		canBashDoors = false;
		canBashFences = false;
		canUseRangedWeapon = true;
		haulDroppedApparel = false;
		restUntilHealed = false;
		ignoreJoyTimeAssignment = false;
		doUntilGatheringEnded = false;
		overeat = false;
		ingestTotalCount = false;
		attackDoorIfTargetLost = false;
		takeExtraIngestibles = 0;
		expireRequiresEnemiesNearby = false;
		lord = null;
		collideWithPawns = false;
		forceSleep = false;
		interaction = null;
		endIfCantShootTargetFromCurPos = false;
		endIfCantShootInMelee = false;
		checkEncumbrance = false;
		followRadius = 0f;
		endAfterTendedOnce = false;
		quest = null;
		mote = null;
		reactingToMeleeThreat = false;
		wasOnMeditationTimeAssignment = false;
		psyfocusTargetLast = -1f;
		preventFriendlyFire = false;
		ropingPriority = RopingPriority.Closest;
		ropeToUnenclosedPens = false;
		thingDefToCarry = null;
		dutyTag = null;
		ritualTag = null;
		controlGroupTag = null;
		lookDirection = Direction8Way.Invalid;
		overrideFacing = Rot4.Invalid;
		forceMaintainFacing = false;
		takeInventoryDelay = 0;
		draftedTend = false;
		showSpeechBubbles = true;
		speechSoundFemale = null;
		speechSoundMale = null;
		speechFaceSpectatorsIfPossible = false;
		biosculpterCycleKey = null;
		startInvoluntarySleep = false;
		reportStringOverride = null;
		crawlingReportStringOverride = null;
		isLearningDesire = false;
		flying = false;
		swimming = false;
		intervalScalingTarget = TargetIndex.None;
		ensureReachable = false;
		jobGiverThinkTree = null;
		jobGiver = null;
		workGiverDef = null;
		ability = null;
		source = null;
		if (cachedDriver != null)
		{
			cachedDriver.job = null;
		}
		cachedDriver = null;
		if (lastJobDriverMade != null)
		{
			lastJobDriverMade.job = null;
		}
		lastJobDriverMade = null;
	}

	public Job()
	{
	}

	public Job(JobDef def)
		: this(def, null)
	{
	}

	public Job(JobDef def, LocalTargetInfo targetA)
		: this(def, targetA, null)
	{
	}

	public Job(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB)
	{
		this.def = def;
		this.targetA = targetA;
		this.targetB = targetB;
		loadID = Find.UniqueIDsManager.GetNextJobID();
	}

	public Job(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
	{
		this.def = def;
		this.targetA = targetA;
		this.targetB = targetB;
		this.targetC = targetC;
		loadID = Find.UniqueIDsManager.GetNextJobID();
	}

	public Job(JobDef def, LocalTargetInfo targetA, int expiryInterval, bool checkOverrideOnExpiry = false)
	{
		this.def = def;
		this.targetA = targetA;
		this.expiryInterval = expiryInterval;
		checkOverrideOnExpire = checkOverrideOnExpiry;
		loadID = Find.UniqueIDsManager.GetNextJobID();
	}

	public Job(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
	{
		this.def = def;
		this.expiryInterval = expiryInterval;
		checkOverrideOnExpire = checkOverrideOnExpiry;
		loadID = Find.UniqueIDsManager.GetNextJobID();
	}

	public Job Clone()
	{
		return new Job
		{
			def = def,
			targetA = targetA,
			targetB = targetB,
			targetC = targetC,
			targetQueueA = targetQueueA,
			targetQueueB = targetQueueB,
			globalTarget = globalTarget,
			count = count,
			countQueue = countQueue,
			loadID = loadID,
			expiryInterval = expiryInterval,
			checkOverrideOnExpire = checkOverrideOnExpire,
			playerForced = playerForced,
			showCarryingInspectLine = showCarryingInspectLine,
			placedThings = placedThings,
			maxNumMeleeAttacks = maxNumMeleeAttacks,
			maxNumStaticAttacks = maxNumStaticAttacks,
			locomotionUrgency = locomotionUrgency,
			haulMode = haulMode,
			bill = bill,
			commTarget = commTarget,
			plantDefToSow = plantDefToSow,
			thingDefToCarry = thingDefToCarry,
			verbToUse = verbToUse,
			haulOpportunisticDuplicates = haulOpportunisticDuplicates,
			exitMapOnArrival = exitMapOnArrival,
			failIfCantJoinOrCreateCaravan = failIfCantJoinOrCreateCaravan,
			killIncappedTarget = killIncappedTarget,
			ignoreForbidden = ignoreForbidden,
			ignoreDesignations = ignoreDesignations,
			canBashDoors = canBashDoors,
			canBashFences = canBashFences,
			canUseRangedWeapon = canUseRangedWeapon,
			haulDroppedApparel = haulDroppedApparel,
			restUntilHealed = restUntilHealed,
			ignoreJoyTimeAssignment = ignoreJoyTimeAssignment,
			doUntilGatheringEnded = doUntilGatheringEnded,
			overeat = overeat,
			ingestTotalCount = ingestTotalCount,
			attackDoorIfTargetLost = attackDoorIfTargetLost,
			takeExtraIngestibles = takeExtraIngestibles,
			expireRequiresEnemiesNearby = expireRequiresEnemiesNearby,
			expireOnEnemiesNearby = expireOnEnemiesNearby,
			intervalScalingTarget = intervalScalingTarget,
			lord = lord,
			collideWithPawns = collideWithPawns,
			forceSleep = forceSleep,
			interaction = interaction,
			endIfCantShootTargetFromCurPos = endIfCantShootTargetFromCurPos,
			endIfCantShootInMelee = endIfCantShootInMelee,
			checkEncumbrance = checkEncumbrance,
			followRadius = followRadius,
			endAfterTendedOnce = endAfterTendedOnce,
			quest = quest,
			mote = mote,
			psyfocusTargetLast = psyfocusTargetLast,
			wasOnMeditationTimeAssignment = wasOnMeditationTimeAssignment,
			reactingToMeleeThreat = reactingToMeleeThreat,
			preventFriendlyFire = preventFriendlyFire,
			ropingPriority = ropingPriority,
			ropeToUnenclosedPens = ropeToUnenclosedPens,
			showSpeechBubbles = showSpeechBubbles,
			lookDirection = lookDirection,
			overrideFacing = overrideFacing,
			forceMaintainFacing = forceMaintainFacing,
			dutyTag = dutyTag,
			ritualTag = ritualTag,
			controlGroupTag = controlGroupTag,
			takeInventoryDelay = takeInventoryDelay,
			draftedTend = draftedTend,
			speechFaceSpectatorsIfPossible = speechFaceSpectatorsIfPossible,
			speechSoundMale = speechSoundMale,
			speechSoundFemale = speechSoundFemale,
			biosculpterCycleKey = biosculpterCycleKey,
			reportStringOverride = reportStringOverride,
			crawlingReportStringOverride = crawlingReportStringOverride,
			startInvoluntarySleep = startInvoluntarySleep,
			isLearningDesire = isLearningDesire,
			flying = flying,
			swimming = swimming,
			ensureReachable = ensureReachable,
			jobGiverThinkTree = jobGiverThinkTree,
			jobGiver = jobGiver,
			workGiverDef = workGiverDef,
			ability = ability,
			source = source
		};
	}

	public LocalTargetInfo GetTarget(TargetIndex ind)
	{
		return ind switch
		{
			TargetIndex.A => targetA, 
			TargetIndex.B => targetB, 
			TargetIndex.C => targetC, 
			_ => throw new ArgumentException(), 
		};
	}

	public List<LocalTargetInfo> GetTargetQueue(TargetIndex ind)
	{
		switch (ind)
		{
		case TargetIndex.A:
			if (targetQueueA == null)
			{
				targetQueueA = new List<LocalTargetInfo>();
			}
			return targetQueueA;
		case TargetIndex.B:
			if (targetQueueB == null)
			{
				targetQueueB = new List<LocalTargetInfo>();
			}
			return targetQueueB;
		default:
			throw new ArgumentException();
		}
	}

	public void SetTarget(TargetIndex ind, LocalTargetInfo pack)
	{
		switch (ind)
		{
		case TargetIndex.A:
			targetA = pack;
			break;
		case TargetIndex.B:
			targetB = pack;
			break;
		case TargetIndex.C:
			targetC = pack;
			break;
		default:
			throw new ArgumentException();
		}
	}

	public void AddQueuedTarget(TargetIndex ind, LocalTargetInfo target)
	{
		GetTargetQueue(ind).Add(target);
	}

	public void ExposeData()
	{
		ILoadReferenceable refee = (ILoadReferenceable)commTarget;
		Scribe_References.Look(ref refee, "commTarget");
		commTarget = (ICommunicable)refee;
		Scribe_References.Look(ref verbToUse, "verbToUse");
		Scribe_References.Look(ref bill, "bill");
		Scribe_References.Look(ref lord, "lord");
		Scribe_References.Look(ref quest, "quest");
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_TargetInfo.Look(ref targetA, "targetA");
		Scribe_TargetInfo.Look(ref targetB, "targetB");
		Scribe_TargetInfo.Look(ref targetC, "targetC");
		Scribe_TargetInfo.Look(ref globalTarget, "globalTarget");
		Scribe_Collections.Look(ref targetQueueA, "targetQueueA", LookMode.Undefined);
		Scribe_Collections.Look(ref targetQueueB, "targetQueueB", LookMode.Undefined);
		Scribe_Values.Look(ref count, "count", -1);
		Scribe_Collections.Look(ref countQueue, "countQueue", LookMode.Undefined);
		Scribe_Values.Look(ref ignoreForbidden, "ignoreForbidden", defaultValue: false);
		Scribe_Values.Look(ref startTick, "startTick", -1);
		Scribe_Values.Look(ref expiryInterval, "expiryInterval", -1);
		Scribe_Values.Look(ref checkOverrideOnExpire, "checkOverrideOnExpire", defaultValue: false);
		Scribe_Values.Look(ref playerForced, "playerForced", defaultValue: false);
		Scribe_Values.Look(ref playerInterruptedForced, "playerInterruptedForced", defaultValue: false);
		Scribe_Values.Look(ref intervalScalingTarget, "intervalScalingTarget", TargetIndex.None);
		Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Undefined);
		Scribe_Values.Look(ref maxNumMeleeAttacks, "maxNumMeleeAttacks", int.MaxValue);
		Scribe_Values.Look(ref maxNumStaticAttacks, "maxNumStaticAttacks", int.MaxValue);
		Scribe_Values.Look(ref exitMapOnArrival, "exitMapOnArrival", defaultValue: false);
		Scribe_Values.Look(ref failIfCantJoinOrCreateCaravan, "failIfCantJoinOrCreateCaravan", defaultValue: false);
		Scribe_Values.Look(ref killIncappedTarget, "killIncappedTarget", defaultValue: false);
		Scribe_Values.Look(ref haulOpportunisticDuplicates, "haulOpportunisticDuplicates", defaultValue: false);
		Scribe_Values.Look(ref haulMode, "haulMode", HaulMode.Undefined);
		Scribe_Defs.Look(ref plantDefToSow, "plantDefToSow");
		Scribe_Defs.Look(ref thingDefToCarry, "thingDefToCarry");
		Scribe_Values.Look(ref locomotionUrgency, "locomotionUrgency", LocomotionUrgency.Jog);
		Scribe_Values.Look(ref ignoreDesignations, "ignoreDesignations", defaultValue: false);
		Scribe_Values.Look(ref canBashDoors, "canBash", defaultValue: false);
		Scribe_Values.Look(ref canBashFences, "canBashFences", defaultValue: false);
		Scribe_Values.Look(ref canUseRangedWeapon, "canUseRangedWeapon", defaultValue: true);
		Scribe_Values.Look(ref haulDroppedApparel, "haulDroppedApparel", defaultValue: false);
		Scribe_Values.Look(ref restUntilHealed, "restUntilHealed", defaultValue: false);
		Scribe_Values.Look(ref ignoreJoyTimeAssignment, "ignoreJoyTimeAssignment", defaultValue: false);
		Scribe_Values.Look(ref overeat, "overeat", defaultValue: false);
		Scribe_Values.Look(ref ingestTotalCount, "ingestTotalCount", defaultValue: false);
		Scribe_Values.Look(ref attackDoorIfTargetLost, "attackDoorIfTargetLost", defaultValue: false);
		Scribe_Values.Look(ref takeExtraIngestibles, "takeExtraIngestibles", 0);
		Scribe_Values.Look(ref expireRequiresEnemiesNearby, "expireRequiresEnemiesNearby", defaultValue: false);
		Scribe_Values.Look(ref collideWithPawns, "collideWithPawns", defaultValue: false);
		Scribe_Values.Look(ref forceSleep, "forceSleep", defaultValue: false);
		Scribe_Defs.Look(ref interaction, "interaction");
		Scribe_Values.Look(ref endIfCantShootTargetFromCurPos, "endIfCantShootTargetFromCurPos", defaultValue: false);
		Scribe_Values.Look(ref endIfCantShootInMelee, "endIfCantShootInMelee", defaultValue: false);
		Scribe_Values.Look(ref checkEncumbrance, "checkEncumbrance", defaultValue: false);
		Scribe_Values.Look(ref followRadius, "followRadius", 0f);
		Scribe_Values.Look(ref endAfterTendedOnce, "endAfterTendedOnce", defaultValue: false);
		Scribe_Defs.Look(ref workGiverDef, "workGiverDef");
		Scribe_Defs.Look(ref jobGiverThinkTree, "jobGiverThinkTree");
		Scribe_Values.Look(ref doUntilGatheringEnded, "doUntilGatheringEnded", defaultValue: false);
		Scribe_Values.Look(ref psyfocusTargetLast, "psyfocusTargetLast", 0f);
		Scribe_Values.Look(ref wasOnMeditationTimeAssignment, "wasOnMeditationTimeAssignment", defaultValue: false);
		Scribe_Values.Look(ref reactingToMeleeThreat, "reactingToMeleeThreat", defaultValue: false);
		Scribe_Values.Look(ref preventFriendlyFire, "preventFriendlyFire", defaultValue: false);
		Scribe_Values.Look(ref ropingPriority, "ropingPriority", RopingPriority.Closest);
		Scribe_Values.Look(ref ropeToUnenclosedPens, "ropeToUnenclosedPens", defaultValue: false);
		Scribe_Values.Look(ref lookDirection, "lookDirection", Direction8Way.Invalid);
		Scribe_Values.Look(ref dutyTag, "dutyTag");
		Scribe_Values.Look(ref ritualTag, "ritualTag");
		Scribe_Values.Look(ref controlGroupTag, "controlGroupTag");
		Scribe_References.Look(ref ability, "ability");
		Scribe_References.Look(ref source, "source");
		Scribe_Values.Look(ref takeInventoryDelay, "takeInventoryDelay", 0);
		Scribe_Values.Look(ref draftedTend, "draftedTend", defaultValue: false);
		Scribe_Values.Look(ref showSpeechBubbles, "showSpeechBubbles", defaultValue: true);
		Scribe_Values.Look(ref overrideFacing, "overrideFacing", Rot4.Invalid);
		Scribe_Values.Look(ref forceMaintainFacing, "forceMaintainFacing", defaultValue: false);
		Scribe_Values.Look(ref speechFaceSpectatorsIfPossible, "speechFaceSpectatorsIfPossible", defaultValue: false);
		Scribe_Defs.Look(ref speechSoundMale, "speechSoundMale");
		Scribe_Defs.Look(ref speechSoundFemale, "speechSoundFemale");
		Scribe_Values.Look(ref biosculpterCycleKey, "biosculpterCycleKey");
		Scribe_Values.Look(ref startInvoluntarySleep, "startInvoluntarySleep", defaultValue: false);
		Scribe_Values.Look(ref reportStringOverride, "reportStringOverride");
		Scribe_Values.Look(ref crawlingReportStringOverride, "crawlingReportStringOverride");
		Scribe_Values.Look(ref isLearningDesire, "isLearning", defaultValue: false);
		Scribe_Values.Look(ref interactableIndex, "interactableIndex", 0);
		Scribe_Values.Look(ref ensureReachable, "ensureReachable", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			jobGiverKey = jobGiver?.UniqueSaveKey ?? (-1);
		}
		Scribe_Values.Look(ref jobGiverKey, "lastJobGiverKey", -1);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (jobGiverKey != -1 && !jobGiverThinkTree.TryGetThinkNodeWithSaveKey(jobGiverKey, out jobGiver))
			{
				Log.Warning("Could not find think node with key " + jobGiverKey);
			}
			if (verbToUse != null && verbToUse.BuggedAfterLoading)
			{
				verbToUse = null;
				Log.Warning(GetType()?.ToString() + " had a bugged verbToUse after loading.");
			}
		}
	}

	public JobDriver MakeDriver(Pawn driverPawn)
	{
		JobDriver jobDriver = (JobDriver)Activator.CreateInstance(def.driverClass);
		jobDriver.pawn = driverPawn;
		jobDriver.job = this;
		lastJobDriverMade = jobDriver;
		return jobDriver;
	}

	public JobDriver GetCachedDriver(Pawn driverPawn)
	{
		if (cachedDriver == null)
		{
			cachedDriver = MakeDriver(driverPawn);
		}
		if (cachedDriver.pawn != driverPawn)
		{
			Log.Error("Tried to use the same driver for 2 pawns: " + cachedDriver.ToStringSafe() + ", first pawn= " + cachedDriver.pawn.ToStringSafe() + ", second pawn=" + driverPawn.ToStringSafe());
		}
		return cachedDriver;
	}

	public bool TryMakePreToilReservations(Pawn driverPawn, bool errorOnFailed)
	{
		return GetCachedDriver(driverPawn).TryMakePreToilReservations(errorOnFailed);
	}

	public string GetReport(Pawn driverPawn)
	{
		return GetCachedDriver(driverPawn).GetReport();
	}

	public LocalTargetInfo GetDestination(Pawn driverPawn)
	{
		return targetA;
	}

	public bool CanBeginNow(Pawn pawn, bool whileLyingDown = false)
	{
		if (pawn.Downed)
		{
			whileLyingDown = true;
		}
		if (whileLyingDown)
		{
			return GetCachedDriver(pawn).CanBeginNowWhileLyingDown();
		}
		return true;
	}

	public bool JobIsSameAs(Pawn pawn, Job other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (def != other.def || verbToUse != other.verbToUse || bill != other.bill)
		{
			return false;
		}
		bool? flag = GetCachedDriver(pawn).IsSameJobAs(other);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		if (targetA != other.targetA || targetB != other.targetB || targetC != other.targetC || commTarget != other.commTarget)
		{
			return false;
		}
		return true;
	}

	public bool AnyTargetIs(LocalTargetInfo target)
	{
		if (!target.IsValid)
		{
			return false;
		}
		if (!(targetA == target) && !(targetB == target) && !(targetC == target) && (targetQueueA == null || !targetQueueA.Contains(target)))
		{
			if (targetQueueB != null)
			{
				return targetQueueB.Contains(target);
			}
			return false;
		}
		return true;
	}

	public bool AnyTargetOutsideArea(Area zone)
	{
		if (IsTargetOutsideArea(targetA, zone) || IsTargetOutsideArea(targetB, zone) || IsTargetOutsideArea(targetC, zone))
		{
			return true;
		}
		if (targetQueueA != null)
		{
			foreach (LocalTargetInfo item in targetQueueA)
			{
				if (IsTargetOutsideArea(item, zone))
				{
					return true;
				}
			}
		}
		if (targetQueueB != null)
		{
			foreach (LocalTargetInfo item2 in targetQueueB)
			{
				if (IsTargetOutsideArea(item2, zone))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsTargetOutsideArea(LocalTargetInfo target, Area zone)
	{
		IntVec3 cell = target.Cell;
		if (cell.IsValid)
		{
			return !zone[cell];
		}
		return false;
	}

	public override string ToString()
	{
		string text = $"{def} ({GetUniqueLoadID()})";
		if (targetA.IsValid)
		{
			text += $" A = {targetA}";
		}
		if (targetB.IsValid)
		{
			text += $" B = {targetB}";
		}
		if (targetC.IsValid)
		{
			text += $" C = {targetC}";
		}
		if (jobGiver != null)
		{
			text = text + " Giver = " + jobGiver.GetType().Name + " [workGiverDef: " + ((workGiverDef == null) ? "null" : workGiverDef.ToString()) + "]";
		}
		return text;
	}

	public string GetUniqueLoadID()
	{
		return "Job_" + loadID;
	}

	public void LogDetails(Pawn pawn)
	{
		string text = $"{this}";
		JobDriver curDriver = pawn.jobs.curDriver;
		text = ((curDriver == null) ? (text + "\nno driver.") : (text + $"\ndriver details - current toil [{curDriver.CurToilIndex}]: {curDriver.CurToilString}"));
		Log.Message(text);
	}
}
