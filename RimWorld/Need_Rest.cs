using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Need_Rest : Need
{
	private int lastRestTick = -999;

	private float lastRestEffectiveness = 1f;

	private int ticksAtZero;

	private const float FullSleepHours = 10.5f;

	public const float BaseRestGainPerTick = 3.809524E-05f;

	private const float BaseRestFallPerTick = 1.5833333E-05f;

	public const float ThreshTired = 0.28f;

	public const float ThreshVeryTired = 0.14f;

	public const float DefaultFallAsleepMaxLevel = 0.75f;

	public const float DefaultNaturalWakeThreshold = 1f;

	public const float CanWakeThreshold = 0.2f;

	private const float BaseInvoluntarySleepMTBDays = 0.25f;

	public RestCategory CurCategory
	{
		get
		{
			if (CurLevel < 0.01f)
			{
				return RestCategory.Exhausted;
			}
			if (CurLevel < 0.14f)
			{
				return RestCategory.VeryTired;
			}
			if (CurLevel < 0.28f)
			{
				return RestCategory.Tired;
			}
			return RestCategory.Rested;
		}
	}

	public float RestFallPerTick => CurCategory switch
	{
		RestCategory.Rested => 1.5833333E-05f * RestFallFactor, 
		RestCategory.Tired => 1.5833333E-05f * RestFallFactor * 0.7f, 
		RestCategory.VeryTired => 1.5833333E-05f * RestFallFactor * 0.3f, 
		RestCategory.Exhausted => 1.5833333E-05f * RestFallFactor * 0.6f, 
		_ => 999f, 
	};

	private float RestFallFactor => pawn.health.hediffSet.RestFallFactor;

	public override int GUIChangeArrow
	{
		get
		{
			if (!Resting)
			{
				return -1;
			}
			return 1;
		}
	}

	public int TicksAtZero => ticksAtZero;

	public bool Resting => Find.TickManager.TicksGame < lastRestTick + pawn.UpdateRateTicks;

	public Need_Rest(Pawn pawn)
		: base(pawn)
	{
		threshPercents = new List<float>();
		threshPercents.Add(0.28f);
		threshPercents.Add(0.14f);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksAtZero, "ticksAtZero", 0);
	}

	public override void SetInitialLevel()
	{
		CurLevel = Rand.Range(0.9f, 1f);
	}

	public override void NeedInterval()
	{
		if (!IsFrozen)
		{
			if (Resting)
			{
				float num = lastRestEffectiveness;
				num *= pawn.GetStatValue(StatDefOf.RestRateMultiplier);
				if (num > 0f)
				{
					CurLevel += 0.005714286f * num;
				}
			}
			else
			{
				float num2 = RestFallPerTick * 150f * pawn.GetStatValue(StatDefOf.RestFallRateFactor);
				CurLevel -= num2;
			}
		}
		if (CurLevel < 0.0001f)
		{
			ticksAtZero += 150;
		}
		else
		{
			ticksAtZero = 0;
		}
		if (!CanInvoluntarilySleep(pawn) || !ShouldInvoluntarySleepFromMTB())
		{
			return;
		}
		Building_Bed building_Bed = pawn.CurrentBed();
		LocalTargetInfo targetA;
		if (building_Bed != null)
		{
			targetA = building_Bed;
		}
		else
		{
			Thing spawnedParentOrMe = pawn.SpawnedParentOrMe;
			targetA = ((spawnedParentOrMe == null || spawnedParentOrMe == pawn) ? ((LocalTargetInfo)pawn.Position) : ((LocalTargetInfo)spawnedParentOrMe));
		}
		Job job = JobMaker.MakeJob(JobDefOf.LayDown, targetA);
		job.startInvoluntarySleep = true;
		pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.SatisfyingNeeds, fromQueue: false, canReturnCurJobToPool: false, null, continueSleeping: false, addToJobsThisTick: true, preToilReservationsCanFail: true);
		if (pawn.InMentalState && pawn.MentalStateDef.recoverFromCollapsingExhausted)
		{
			pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
		}
		LifeStageDef curLifeStage = pawn.ageTracker.CurLifeStage;
		if (curLifeStage == null || curLifeStage.involuntarySleepIsNegativeEvent)
		{
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessageInvoluntarySleep".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.NegativeEvent);
			}
			TaleRecorder.RecordTale(TaleDefOf.Exhausted, pawn);
		}
	}

	public void TickResting(float restEffectiveness)
	{
		if (!(restEffectiveness <= 0f))
		{
			lastRestTick = Find.TickManager.TicksGame;
			lastRestEffectiveness = restEffectiveness;
		}
	}

	private bool ShouldInvoluntarySleepFromMTB()
	{
		float num = float.PositiveInfinity;
		if (ticksAtZero > 1000)
		{
			num = ((ticksAtZero < 15000) ? 0.25f : ((ticksAtZero < 30000) ? 0.125f : ((ticksAtZero >= 45000) ? 0.0625f : (1f / 12f))));
		}
		SimpleCurve simpleCurve = pawn.ageTracker.CurLifeStage?.involuntarySleepMTBDaysFromRest;
		if (simpleCurve != null)
		{
			num = Rand.CombineMTBs(num, simpleCurve.Evaluate(base.CurLevelPercentage));
		}
		return Rand.MTBEventOccurs(num, 60000f, 150f);
	}

	private static bool CanInvoluntarilySleep(Pawn pawn)
	{
		Pawn_JobTracker jobs = pawn.jobs;
		if (jobs != null && jobs.curDriver?.asleep == true)
		{
			return false;
		}
		if (!RestUtility.CanFallAsleep(pawn))
		{
			return false;
		}
		if (!pawn.Spawned)
		{
			if (!pawn.ageTracker.CurLifeStage.canSleepWhileHeld)
			{
				return false;
			}
			if (!(pawn.SpawnedParentOrMe is Pawn))
			{
				return false;
			}
			if (pawn.IsWorldPawn())
			{
				return false;
			}
			if (pawn.IsCaravanMember())
			{
				return false;
			}
		}
		if (pawn.ageTracker.CurLifeStage.canVoluntarilySleep && pawn.CurJobDef == JobDefOf.LayDown)
		{
			return false;
		}
		return true;
	}
}
