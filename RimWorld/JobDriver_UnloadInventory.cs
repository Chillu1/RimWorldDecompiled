using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_UnloadInventory : JobDriver
{
	private const TargetIndex OtherPawnInd = TargetIndex.A;

	private const TargetIndex ItemToHaulInd = TargetIndex.B;

	private const TargetIndex StoreCellInd = TargetIndex.C;

	private const int UnloadDuration = 10;

	private Pawn OtherPawn => job.GetTarget(TargetIndex.A).Pawn;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(OtherPawn, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Wait(10);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			Pawn otherPawn = OtherPawn;
			if (!otherPawn.inventory.UnloadEverything)
			{
				EndJobWith(JobCondition.Succeeded);
			}
			else
			{
				ThingCount firstUnloadableThing = otherPawn.inventory.FirstUnloadableThing;
				if (!firstUnloadableThing.Thing.def.EverStorable(willMinifyIfPossible: false) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, pawn, out var storeCell))
				{
					otherPawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out var lastResultingThing);
					EndJobWith(JobCondition.Succeeded);
					lastResultingThing?.SetForbidden(value: false, warnOnFail: false);
				}
				else
				{
					otherPawn.inventory.innerContainer.TryTransferToContainer(firstUnloadableThing.Thing, pawn.carryTracker.innerContainer, firstUnloadableThing.Count, out var resultingTransferredItem);
					job.count = resultingTransferredItem.stackCount;
					job.SetTarget(TargetIndex.B, resultingTransferredItem);
					job.SetTarget(TargetIndex.C, storeCell);
					firstUnloadableThing.Thing.SetForbidden(value: false, warnOnFail: false);
				}
				if (otherPawn.RaceProps.packAnimal && otherPawn.inventory.innerContainer.Count == 0)
				{
					otherPawn.Drawer.renderer.SetAllGraphicsDirty();
				}
			}
		};
		yield return toil;
		yield return Toils_Reserve.Reserve(TargetIndex.C);
		Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
		yield return carryToCell;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, storageMode: true);
	}
}
