using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FoodFeedPatient : JobDriver
	{
		private const TargetIndex FoodSourceInd = TargetIndex.A;

		private const TargetIndex DelivereeInd = TargetIndex.B;

		private const float FeedDurationMultiplier = 1.5f;

		protected Thing Food => job.targetA.Thing;

		protected Pawn Deliveree => (Pawn)job.targetB.Thing;

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
			if (pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			}
			else if (base.TargetThingA is Building_NutrientPasteDispenser)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
			}
			else
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, Deliveree);
			}
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return Toils_Ingest.ChewIngestible(Deliveree, 1.5f, TargetIndex.A).FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(Deliveree, TargetIndex.A);
		}
	}
}
