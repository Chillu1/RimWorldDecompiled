using System;
using RimWorld;

namespace Verse;

public class Pawn_CallTracker
{
	public Pawn pawn;

	private int ticksToNextCall = -1;

	private static readonly IntRange CallOnAggroDelayRange = new IntRange(0, 120);

	private static readonly IntRange CallOnMeleeDelayRange = new IntRange(0, 20);

	private const float AngryCallOnMeleeChance = 0.5f;

	private const int AggressiveDurationAfterEngagingTarget = 360;

	private bool PawnAggressive
	{
		get
		{
			if (pawn.InAggroMentalState)
			{
				return true;
			}
			if (pawn.mindState.enemyTarget != null && pawn.mindState.enemyTarget.Spawned && Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick <= 360)
			{
				return true;
			}
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.AttackMelee)
			{
				return true;
			}
			return false;
		}
	}

	private float IdleCallVolumeFactor => Find.TickManager.CurTimeSpeed switch
	{
		TimeSpeed.Paused => 1f, 
		TimeSpeed.Normal => 1f, 
		TimeSpeed.Fast => 1f, 
		TimeSpeed.Superfast => 0.25f, 
		TimeSpeed.Ultrafast => 0.25f, 
		_ => throw new NotImplementedException(), 
	};

	public Pawn_CallTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void CallTrackerTickInterval(int delta)
	{
		if (ticksToNextCall < 0)
		{
			ResetTicksToNextCall();
		}
		ticksToNextCall -= delta;
		if (ticksToNextCall <= 0)
		{
			TryDoCall();
			ResetTicksToNextCall();
		}
	}

	private void ResetTicksToNextCall()
	{
		if (pawn.IsMutant)
		{
			ticksToNextCall = MutantUtility.CallIntervalRange.RandomInRange;
			return;
		}
		ticksToNextCall = pawn.def.race.soundCallIntervalRange.RandomInRange;
		if (PawnAggressive)
		{
			ticksToNextCall = (int)((float)ticksToNextCall * pawn.def.race.soundCallIntervalAggressiveFactor);
		}
		else if (pawn.Faction != null && pawn.Faction.IsPlayer)
		{
			ticksToNextCall = (int)((float)ticksToNextCall * pawn.def.race.soundCallIntervalFriendlyFactor);
		}
	}

	private void TryDoCall()
	{
		if (pawn.Spawned && Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(pawn.Position) && !pawn.Downed && pawn.Awake() && !pawn.IsPsychologicallyInvisible() && !pawn.Position.Fogged(pawn.MapHeld) && (!pawn.IsColonyMech || !pawn.IsCharging()))
		{
			DoCall();
		}
	}

	public void DoCall(bool forceAggressive = false)
	{
		if (PawnAggressive || forceAggressive)
		{
			LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge lifeStage) => lifeStage.soundAngry, null, (MutantDef mutant) => mutant.soundAngry);
			return;
		}
		LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge lifeStage) => lifeStage.soundCall, (GeneDef gene) => gene.soundCall, (MutantDef mutant) => mutant.soundCall, IdleCallVolumeFactor);
	}

	public void Notify_InAggroMentalState()
	{
		ticksToNextCall = CallOnAggroDelayRange.RandomInRange;
	}

	public void Notify_DidMeleeAttack()
	{
		if (Rand.Value < 0.5f)
		{
			ticksToNextCall = CallOnMeleeDelayRange.RandomInRange;
		}
	}

	public void Notify_Released()
	{
		if (Rand.Value < 0.75f)
		{
			ticksToNextCall = CallOnAggroDelayRange.RandomInRange;
		}
	}
}
