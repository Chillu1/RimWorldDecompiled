using System;
using RimWorld;

namespace Verse.AI
{
	public abstract class JobGiver_Wander : ThinkNode_JobGiver
	{
		protected float wanderRadius;

		protected Func<Pawn, IntVec3, IntVec3, bool> wanderDestValidator;

		protected IntRange ticksBetweenWandersRange = new IntRange(20, 100);

		protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		protected LocomotionUrgency? locomotionUrgencyOutsideRadius;

		protected Danger maxDanger = Danger.None;

		protected int expiryInterval = -1;

		protected bool canBashDoors;

		protected bool expireOnNearbyEnemy;

		protected bool canEatMealsFromInventory;

		private const int EnemyScanCheckTicks = 30;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Wander obj = (JobGiver_Wander)base.DeepCopy(resolve);
			obj.wanderRadius = wanderRadius;
			obj.wanderDestValidator = wanderDestValidator;
			obj.ticksBetweenWandersRange = ticksBetweenWandersRange;
			obj.locomotionUrgency = locomotionUrgency;
			obj.locomotionUrgencyOutsideRadius = locomotionUrgencyOutsideRadius;
			obj.maxDanger = maxDanger;
			obj.expiryInterval = expiryInterval;
			obj.canBashDoors = canBashDoors;
			obj.expireOnNearbyEnemy = expireOnNearbyEnemy;
			obj.canEatMealsFromInventory = canEatMealsFromInventory;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = pawn.CurJob != null && pawn.CurJob.def == JobDefOf.GotoWander;
			bool flying = pawn.Flying;
			bool num = pawn.mindState.nextMoveOrderIsWait && !flying;
			if (!flag)
			{
				pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
			}
			if (num && !flag)
			{
				return GetWaitJob();
			}
			IntVec3 exactWanderDest = GetExactWanderDest(pawn);
			if (exactWanderDest == pawn.Position && !flag)
			{
				return GetWaitJob();
			}
			if (!exactWanderDest.IsValid)
			{
				pawn.mindState.nextMoveOrderIsWait = false;
				return null;
			}
			LocomotionUrgency value = locomotionUrgency;
			if (locomotionUrgencyOutsideRadius.HasValue && !pawn.Position.InHorDistOf(GetWanderRoot(pawn), wanderRadius))
			{
				value = locomotionUrgencyOutsideRadius.Value;
			}
			Job job = JobMaker.MakeJob(JobDefOf.GotoWander, exactWanderDest);
			job.locomotionUrgency = value;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = true;
			job.reportStringOverride = reportStringOverride;
			job.canBashDoors = canBashDoors;
			if (expireOnNearbyEnemy)
			{
				job.expiryInterval = 30;
				job.checkOverrideOnExpire = true;
			}
			DecorateGotoJob(job);
			return job;
			Job GetWaitJob()
			{
				Job job2 = JobMaker.MakeJob(JobDefOf.Wait_Wander);
				job2.expiryInterval = ticksBetweenWandersRange.RandomInRange;
				job2.reportStringOverride = reportStringOverride;
				if (expireOnNearbyEnemy)
				{
					job2.expiryInterval = 30;
					job2.checkOverrideOnExpire = true;
				}
				return job2;
			}
		}

		protected virtual IntVec3 GetExactWanderDest(Pawn pawn)
		{
			IntVec3 wanderRoot = GetWanderRoot(pawn);
			if (!wanderRoot.IsValid)
			{
				return IntVec3.Invalid;
			}
			float value = wanderRadius;
			PawnDuty duty = pawn.mindState.duty;
			if (duty != null && duty.wanderRadius.HasValue)
			{
				value = duty.wanderRadius.Value;
			}
			return RCellFinder.RandomWanderDestFor(pawn, wanderRoot, value, wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, maxDanger), canBashDoors);
		}

		protected abstract IntVec3 GetWanderRoot(Pawn pawn);

		protected virtual void DecorateGotoJob(Job job)
		{
		}
	}
}
