using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TakeFromOtherInventory : JobDriver
	{
		public const TargetIndex ItemIndex = TargetIndex.A;

		public const TargetIndex ItemHolderIndex = TargetIndex.B;

		public Pawn ItemHoldingPawn => job.GetTarget(TargetIndex.B).Pawn;

		public Pawn_InventoryTracker ItemHoldingInventory => base.TargetThingA.ParentHolder as Pawn_InventoryTracker;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOn(() => ItemHoldingPawn != ItemHoldingInventory?.pawn || ItemHoldingPawn.IsForbidden(pawn));
			yield return Toils_Haul.TakeFromOtherInventory(base.TargetThingA, pawn.inventory.innerContainer, ItemHoldingInventory.innerContainer);
		}
	}
}
