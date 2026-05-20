using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_FoodDeliver : JobDriver
{
	private bool usingNutrientPasteDispenser;

	private bool eatingFromInventory;

	private const TargetIndex FoodSourceInd = TargetIndex.A;

	private const TargetIndex DelivereeInd = TargetIndex.B;

	private Pawn Deliveree => (Pawn)job.targetB.Thing;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref usingNutrientPasteDispenser, "usingNutrientPasteDispenser", defaultValue: false);
		Scribe_Values.Look(ref eatingFromInventory, "eatingFromInventory", defaultValue: false);
	}

	public override string GetReport()
	{
		if (job.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser && Deliveree != null)
		{
			return JobUtility.GetResolvedJobReportRaw(job.def.reportString, ThingDefOf.MealNutrientPaste.label, ThingDefOf.MealNutrientPaste, Deliveree.LabelShort, Deliveree, "", "");
		}
		return base.GetReport();
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		usingNutrientPasteDispenser = base.TargetThingA is Building_NutrientPasteDispenser;
		eatingFromInventory = pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Deliveree, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.B);
		if (eatingFromInventory)
		{
			yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
		}
		else if (usingNutrientPasteDispenser)
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
			yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
		}
		else
		{
			yield return Toils_Ingest.ReserveFoodFromStackForIngesting(TargetIndex.A, Deliveree);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
			yield return Toils_Ingest.PickupIngestible(TargetIndex.A, Deliveree);
		}
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			actor.pather.StartPath(curJob.targetC, PathEndMode.OnCell);
		};
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		toil.FailOnDestroyedNullOrForbidden(TargetIndex.B);
		toil.AddFailCondition(delegate
		{
			if (!base.pawn.IsCarryingThing(job.GetTarget(TargetIndex.A).Thing))
			{
				return true;
			}
			Pawn pawn = (Pawn)toil.actor.jobs.curJob.targetB.Thing;
			if (!pawn.IsPrisonerOfColony)
			{
				return true;
			}
			return !pawn.guest.CanBeBroughtFood;
		});
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			pawn.carryTracker.TryDropCarriedThing(toil2.actor.jobs.curJob.targetC.Cell, ThingPlaceMode.Direct, out var _);
		};
		toil2.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil2;
	}
}
