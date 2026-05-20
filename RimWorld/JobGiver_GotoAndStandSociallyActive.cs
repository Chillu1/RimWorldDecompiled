using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GotoAndStandSociallyActive : ThinkNode_JobGiver
	{
		protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		public bool allowUnroofed = true;

		protected int expiryInterval = -1;

		public float desiredRadius = -1f;

		public int minDistanceToOtherReservedCell = -1;

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 dest = GetDest(pawn);
			Job job = JobMaker.MakeJob(JobDefOf.GotoAndBeSociallyActive, dest, dest + pawn.Rotation.FacingCell);
			job.locomotionUrgency = locomotionUrgency;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = true;
			return job;
		}

		public IntVec3 GetDest(Pawn pawn)
		{
			Predicate<IntVec3> validatorRelaxed = (IntVec3 x) => allowUnroofed || !x.Roofed(pawn.Map);
			Predicate<IntVec3> cellValidator = delegate(IntVec3 x)
			{
				if (!RitualUtility.GoodSpectateCellForRitual(x, pawn, pawn.Map))
				{
					return false;
				}
				if (minDistanceToOtherReservedCell > 0)
				{
					foreach (IntVec3 item in GenRadial.RadialCellsAround(x, minDistanceToOtherReservedCell, useCenter: true))
					{
						if (!pawn.CanReserveAndReach(item, PathEndMode.OnCell, pawn.NormalMaxDanger()))
						{
							return false;
						}
					}
				}
				return validatorRelaxed(x);
			};
			if (desiredRadius > 0f && GatheringsUtility.TryFindRandomCellInGatheringAreaWithRadius(pawn, desiredRadius, cellValidator, out var result))
			{
				return result;
			}
			if (GatheringsUtility.TryFindRandomCellInGatheringArea(pawn, validatorRelaxed, out result))
			{
				return result;
			}
			return IntVec3.Invalid;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GotoAndStandSociallyActive obj = (JobGiver_GotoAndStandSociallyActive)base.DeepCopy(resolve);
			obj.locomotionUrgency = locomotionUrgency;
			obj.allowUnroofed = allowUnroofed;
			obj.desiredRadius = desiredRadius;
			obj.minDistanceToOtherReservedCell = minDistanceToOtherReservedCell;
			obj.expiryInterval = expiryInterval;
			return obj;
		}
	}
}
