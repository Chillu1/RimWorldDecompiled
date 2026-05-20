using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_UnloadYourInventory : JobDriver
{
	private int countToDrop = -1;

	private const TargetIndex ItemToHaulInd = TargetIndex.A;

	private const TargetIndex StoreCellInd = TargetIndex.B;

	private const int UnloadDuration = 10;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref countToDrop, "countToDrop", -1);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_General.Wait(10);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (!pawn.inventory.UnloadEverything)
			{
				EndJobWith(JobCondition.Succeeded);
			}
			else
			{
				ThingCount firstUnloadableThing = pawn.inventory.FirstUnloadableThing;
				if (!StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, pawn, out var storeCell))
				{
					pawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out var _);
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					job.SetTarget(TargetIndex.A, firstUnloadableThing.Thing);
					job.SetTarget(TargetIndex.B, storeCell);
					countToDrop = firstUnloadableThing.Count;
				}
			}
		};
		yield return toil;
		yield return Toils_Reserve.Reserve(TargetIndex.B);
		yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			Thing resultingTransferredItem = job.GetTarget(TargetIndex.A).Thing;
			if (resultingTransferredItem == null || !pawn.inventory.innerContainer.Contains(resultingTransferredItem))
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !resultingTransferredItem.def.EverStorable(willMinifyIfPossible: false))
				{
					pawn.inventory.innerContainer.TryDrop(resultingTransferredItem, ThingPlaceMode.Near, countToDrop, out resultingTransferredItem);
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					pawn.inventory.innerContainer.TryTransferToContainer(resultingTransferredItem, pawn.carryTracker.innerContainer, countToDrop, out resultingTransferredItem);
					job.count = countToDrop;
					job.SetTarget(TargetIndex.A, resultingTransferredItem);
				}
				resultingTransferredItem.SetForbidden(value: false, warnOnFail: false);
			}
		};
		yield return toil2;
		Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
		yield return carryToCell;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
	}
}
