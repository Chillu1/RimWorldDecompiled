using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (!pawn.inventory.UnloadEverything)
				{
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					ThingCount firstUnloadableThing = pawn.inventory.FirstUnloadableThing;
					if (!StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, pawn, out IntVec3 storeCell))
					{
						pawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out Thing _);
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
			Toil toil2 = new Toil();
			toil2.initAction = delegate
			{
				Thing lastResultingThing = job.GetTarget(TargetIndex.A).Thing;
				if (lastResultingThing == null || !pawn.inventory.innerContainer.Contains(lastResultingThing))
				{
					EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !lastResultingThing.def.EverStorable(willMinifyIfPossible: false))
					{
						pawn.inventory.innerContainer.TryDrop(lastResultingThing, ThingPlaceMode.Near, countToDrop, out lastResultingThing);
						EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						pawn.inventory.innerContainer.TryTransferToContainer(lastResultingThing, pawn.carryTracker.innerContainer, countToDrop, out lastResultingThing);
						job.count = countToDrop;
						job.SetTarget(TargetIndex.A, lastResultingThing);
					}
					lastResultingThing.SetForbidden(value: false, warnOnFail: false);
				}
			};
			yield return toil2;
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
		}
	}
}
