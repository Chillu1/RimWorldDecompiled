using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_FoodFeedPatient : JobDriver
{
	private const TargetIndex FoodSourceInd = TargetIndex.A;

	private const TargetIndex DelivereeInd = TargetIndex.B;

	private const TargetIndex FoodHolderInd = TargetIndex.C;

	private const float FeedDurationMultiplier = 1.5f;

	private const float MetalhorrorInfectionChance = 0.3f;

	protected Thing Food => job.targetA.Thing;

	protected Pawn Deliveree => job.targetB.Pawn;

	protected Pawn_InventoryTracker FoodHolderInventory => Food?.ParentHolder as Pawn_InventoryTracker;

	protected Pawn FoodHolder => job.targetC.Pawn;

	public override string GetReport()
	{
		if (job.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser && Deliveree != null)
		{
			return JobUtility.GetResolvedJobReportRaw(job.def.reportString, ThingDefOf.MealNutrientPaste.label, ThingDefOf.MealNutrientPaste, Deliveree.LabelShort, Deliveree, "", "");
		}
		return base.GetReport();
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(Deliveree, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (!(base.TargetThingA is Building_NutrientPasteDispenser) && (pawn.inventory == null || !pawn.inventory.Contains(base.TargetThingA)))
		{
			int maxAmountToPickup = FoodUtility.GetMaxAmountToPickup(Food, pawn, job.count);
			if (!pawn.Reserve(Food, job, 10, maxAmountToPickup, null, errorOnFailed))
			{
				return false;
			}
			job.count = maxAmountToPickup;
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
		this.FailOn(() => !FoodUtility.ShouldBeFedBySomeone(Deliveree));
		Toil carryFoodFromInventory = Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
		Toil goToNutrientDispenser = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
		Toil goToFoodHolder = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch).FailOn(() => FoodHolder != FoodHolderInventory?.pawn || FoodHolder.IsForbidden(pawn));
		Toil carryFoodToPatient = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
		yield return Toils_Jump.JumpIf(carryFoodFromInventory, () => pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA));
		yield return Toils_Haul.CheckItemCarriedByOtherPawn(Food, TargetIndex.C, goToFoodHolder);
		yield return Toils_Jump.JumpIf(goToNutrientDispenser, () => base.TargetThingA is Building_NutrientPasteDispenser);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
		yield return Toils_Ingest.PickupIngestible(TargetIndex.A, Deliveree);
		yield return Toils_Jump.Jump(carryFoodToPatient);
		yield return goToFoodHolder;
		yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.C);
		yield return Toils_Haul.TakeFromOtherInventory(Food, pawn.inventory.innerContainer, FoodHolderInventory?.innerContainer, job.count, TargetIndex.A);
		yield return carryFoodFromInventory;
		yield return Toils_Jump.Jump(carryFoodToPatient);
		yield return goToNutrientDispenser;
		yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
		yield return carryFoodToPatient;
		yield return Toils_Ingest.ChewIngestible(Deliveree, 1.5f, TargetIndex.A).FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
		Toil toil = Toils_Ingest.FinalizeIngest(Deliveree, TargetIndex.A);
		toil.finishActions = new List<Action>
		{
			delegate
			{
				if (ModsConfig.AnomalyActive && Rand.Chance(0.3f) && MetalhorrorUtility.IsInfected(pawn))
				{
					MetalhorrorUtility.Infect(Deliveree, pawn, "FeedingImplant");
				}
			}
		};
		yield return toil;
	}
}
