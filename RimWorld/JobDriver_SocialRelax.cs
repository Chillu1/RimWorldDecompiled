using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SocialRelax : JobDriver
	{
		private const TargetIndex GatherSpotParentInd = TargetIndex.A;

		private const TargetIndex ChairOrSpotInd = TargetIndex.B;

		private const TargetIndex OptionalIngestibleInd = TargetIndex.C;

		private Thing GatherSpotParent => job.GetTarget(TargetIndex.A).Thing;

		private bool HasChair => job.GetTarget(TargetIndex.B).HasThing;

		private bool HasDrink => job.GetTarget(TargetIndex.C).HasThing;

		private IntVec3 ClosestGatherSpotParentCell => GatherSpotParent.OccupiedRect().ClosestCellTo(pawn.Position);

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (HasDrink && !pawn.Reserve(job.GetTarget(TargetIndex.C), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A);
			if (HasChair)
			{
				this.EndOnDespawnedOrNull(TargetIndex.B);
			}
			if (HasDrink)
			{
				this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.C);
				yield return Toils_Haul.StartCarryThing(TargetIndex.C);
			}
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(ClosestGatherSpotParentCell);
				pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.GoToNextToil);
			};
			toil.handlingFacing = true;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = job.def.joyDuration;
			toil.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			toil.socialMode = RandomSocialMode.SuperActive;
			Toils_Ingest.AddIngestionEffects(toil, pawn, TargetIndex.C, TargetIndex.None);
			yield return toil;
			if (HasDrink)
			{
				yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.C);
			}
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 closestGatherSpotParentCell = ClosestGatherSpotParentCell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, closestGatherSpotParentCell, pawn);
		}
	}
}
