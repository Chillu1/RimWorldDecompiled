using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Ingest : JobDriver
	{
		private bool usingNutrientPasteDispenser;

		private bool eatingFromInventory;

		public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

		public const TargetIndex IngestibleSourceInd = TargetIndex.A;

		private const TargetIndex TableCellInd = TargetIndex.B;

		private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

		private Thing IngestibleSource => job.GetTarget(TargetIndex.A).Thing;

		private float ChewDurationMultiplier
		{
			get
			{
				Thing ingestibleSource = IngestibleSource;
				if (ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat)
				{
					return 1f;
				}
				return 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref usingNutrientPasteDispenser, "usingNutrientPasteDispenser", defaultValue: false);
			Scribe_Values.Look(ref eatingFromInventory, "eatingFromInventory", defaultValue: false);
		}

		public override string GetReport()
		{
			if (usingNutrientPasteDispenser)
			{
				return JobUtility.GetResolvedJobReportRaw(job.def.reportString, ThingDefOf.MealNutrientPaste.label, ThingDefOf.MealNutrientPaste, "", "", "", "");
			}
			Thing thing = job.targetA.Thing;
			if (thing != null && thing.def.ingestible != null)
			{
				if (!thing.def.ingestible.ingestReportStringEat.NullOrEmpty() && (thing.def.ingestible.ingestReportString.NullOrEmpty() || (int)pawn.RaceProps.intelligence < 1))
				{
					return thing.def.ingestible.ingestReportStringEat.Formatted(job.targetA.Thing.LabelShort, job.targetA.Thing);
				}
				if (!thing.def.ingestible.ingestReportString.NullOrEmpty())
				{
					return thing.def.ingestible.ingestReportString.Formatted(job.targetA.Thing.LabelShort, job.targetA.Thing);
				}
			}
			return base.GetReport();
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			usingNutrientPasteDispenser = (IngestibleSource is Building_NutrientPasteDispenser);
			eatingFromInventory = (pawn.inventory != null && pawn.inventory.Contains(IngestibleSource));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Faction != null && !(IngestibleSource is Building_NutrientPasteDispenser))
			{
				Thing ingestibleSource = IngestibleSource;
				if (!pawn.Reserve(ingestibleSource, job, 10, FoodUtility.GetMaxAmountToPickup(ingestibleSource, pawn, job.count), null, errorOnFailed))
				{
					return false;
				}
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!usingNutrientPasteDispenser)
			{
				this.FailOn(() => !IngestibleSource.Destroyed && !IngestibleSource.IngestibleNow);
			}
			Toil chew = Toils_Ingest.ChewIngestible(pawn, ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !IngestibleSource.Spawned && (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			foreach (Toil item in PrepareToIngestToils(chew))
			{
				yield return item;
			}
			yield return chew;
			yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(chew, () => job.GetTarget(TargetIndex.A).Thing is Corpse && pawn.needs.food.CurLevelPercentage < 0.9f);
		}

		private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
		{
			if (usingNutrientPasteDispenser)
			{
				return PrepareToIngestToils_Dispenser();
			}
			if (pawn.RaceProps.ToolUser)
			{
				return PrepareToIngestToils_ToolUser(chewToil);
			}
			return PrepareToIngestToils_NonToolUser();
		}

		private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
			yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
		}

		private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
		{
			if (eatingFromInventory)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			}
			else
			{
				yield return ReserveFood();
				Toil gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
				yield return Toils_Jump.JumpIf(gotoToPickup, () => pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
				yield return Toils_Jump.Jump(chewToil);
				yield return gotoToPickup;
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, pawn);
				Toil reserveExtraFoodToCollect = Toils_Ingest.ReserveFoodFromStackForIngesting(TargetIndex.C);
				Toil findExtraFoodToCollect = new Toil
				{
					initAction = delegate
					{
						if (pawn.inventory.innerContainer.TotalStackCountOfDef(IngestibleSource.def) < job.takeExtraIngestibles)
						{
							Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(IngestibleSource.def), PathEndMode.Touch, TraverseParms.For(pawn), 12f, (Thing x) => pawn.CanReserve(x, 10, 1) && !x.IsForbidden(pawn) && x.IsSociallyProper(pawn));
							if (thing != null)
							{
								job.SetTarget(TargetIndex.C, thing);
								JumpToToil(reserveExtraFoodToCollect);
							}
						}
					},
					defaultCompleteMode = ToilCompleteMode.Instant
				};
				yield return Toils_Jump.Jump(findExtraFoodToCollect);
				yield return reserveExtraFoodToCollect;
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
				yield return Toils_Haul.TakeToInventory(TargetIndex.C, () => job.takeExtraIngestibles - pawn.inventory.innerContainer.TotalStackCountOfDef(IngestibleSource.def));
				yield return findExtraFoodToCollect;
			}
			yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
		}

		private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
		{
			yield return ReserveFood();
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		}

		private Toil ReserveFood()
		{
			return new Toil
			{
				initAction = delegate
				{
					if (pawn.Faction != null)
					{
						Thing thing = job.GetTarget(TargetIndex.A).Thing;
						if (pawn.carryTracker.CarriedThing != thing)
						{
							int maxAmountToPickup = FoodUtility.GetMaxAmountToPickup(thing, pawn, job.count);
							if (maxAmountToPickup != 0)
							{
								if (!pawn.Reserve(thing, job, 10, maxAmountToPickup))
								{
									Log.Error("Pawn food reservation for " + pawn + " on job " + this + " failed, because it could not register food from " + thing + " - amount: " + maxAmountToPickup);
									pawn.jobs.EndCurrentJob(JobCondition.Errored);
								}
								job.count = maxAmountToPickup;
							}
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
			return ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, pawn);
		}

		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
		{
			if (pawn.pather.Moving)
			{
				return false;
			}
			Thing carriedThing = pawn.carryTracker.CarriedThing;
			if (carriedThing == null || !carriedThing.IngestibleNow)
			{
				return false;
			}
			if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable)
			{
				drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
				return true;
			}
			if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
			{
				HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
				if (holdOffset != null)
				{
					drawPos += holdOffset.offset;
					behind = holdOffset.behind;
					flip = holdOffset.flip;
					return true;
				}
			}
			return false;
		}
	}
}
