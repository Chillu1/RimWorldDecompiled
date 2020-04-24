using RimWorld;
using System;

namespace Verse
{
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

		private float IdleCallVolumeFactor
		{
			get
			{
				switch (Find.TickManager.CurTimeSpeed)
				{
				case TimeSpeed.Paused:
					return 1f;
				case TimeSpeed.Normal:
					return 1f;
				case TimeSpeed.Fast:
					return 1f;
				case TimeSpeed.Superfast:
					return 0.25f;
				case TimeSpeed.Ultrafast:
					return 0.25f;
				default:
					throw new NotImplementedException();
				}
			}
		}

		public Pawn_CallTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void CallTrackerTick()
		{
			if (ticksToNextCall < 0)
			{
				ResetTicksToNextCall();
			}
			ticksToNextCall--;
			if (ticksToNextCall <= 0)
			{
				TryDoCall();
				ResetTicksToNextCall();
			}
		}

		private void ResetTicksToNextCall()
		{
			ticksToNextCall = pawn.def.race.soundCallIntervalRange.RandomInRange;
			if (PawnAggressive)
			{
				ticksToNextCall /= 4;
			}
		}

		private void TryDoCall()
		{
			if (Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(pawn.Position) && !pawn.Downed && pawn.Awake() && !pawn.Position.Fogged(pawn.Map))
			{
				DoCall();
			}
		}

		public void DoCall()
		{
			if (pawn.Spawned)
			{
				if (PawnAggressive)
				{
					LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge ls) => ls.soundAngry);
				}
				else
				{
					LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge ls) => ls.soundCall, IdleCallVolumeFactor);
				}
			}
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
}
