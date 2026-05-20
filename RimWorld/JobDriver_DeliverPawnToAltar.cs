using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_DeliverPawnToAltar : JobDriver
	{
		private const TargetIndex TakeeIndex = TargetIndex.A;

		private const TargetIndex TargetCellIndex = TargetIndex.B;

		private const TargetIndex AltarIndex = TargetIndex.C;

		protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		protected Building DropAltar => (Building)job.GetTarget(TargetIndex.C).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			base.Map.reservationManager.ReleaseAllForTarget(Takee);
			if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(DropAltar, job, 1, 0, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!ModLister.CheckIdeology("Deliver to altar job"))
			{
				yield break;
			}
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.C);
			this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.C)
				.FailOn(() => !pawn.CanReach(DropAltar, PathEndMode.OnCell, Danger.Deadly))
				.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A);
			Toil goToAltar = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.ClosestTouch);
			yield return Toils_Jump.JumpIf(goToAltar, () => pawn.IsCarryingPawn(Takee));
			yield return startCarrying;
			yield return goToAltar;
			yield return Toils_General.Do(delegate
			{
				IntVec3 intVec = DropAltar.Position;
				if (DropAltar.def.hasInteractionCell)
				{
					IntVec3 interactionCell = DropAltar.InteractionCell;
					IntVec3 intVec2 = (DropAltar.Position - interactionCell).ClampInsideRect(new CellRect(-1, -1, 3, 3));
					intVec = interactionCell + intVec2;
				}
				else if (DropAltar.def.Size.z % 2 != 0)
				{
					intVec = DropAltar.Position + new IntVec3(0, 0, -DropAltar.def.Size.z / 2).RotatedBy(DropAltar.Rotation);
				}
				job.SetTarget(TargetIndex.B, intVec);
			});
			yield return Toils_Reserve.Release(TargetIndex.C);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, null, storageMode: false);
		}
	}
}
