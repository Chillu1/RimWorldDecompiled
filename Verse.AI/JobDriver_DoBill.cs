using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public class JobDriver_DoBill : JobDriver
	{
		public float workLeft;

		public int billStartTick;

		public int ticksSpentDoingRecipeWork;

		public const PathEndMode GotoIngredientPathEndMode = PathEndMode.ClosestTouch;

		public const TargetIndex BillGiverInd = TargetIndex.A;

		public const TargetIndex IngredientInd = TargetIndex.B;

		public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

		public IBillGiver BillGiver => (job.GetTarget(TargetIndex.A).Thing as IBillGiver) ?? throw new InvalidOperationException("DoBill on non-Billgiver.");

		public override string GetReport()
		{
			if (job.RecipeDef != null)
			{
				return ReportStringProcessed(job.RecipeDef.jobString);
			}
			return base.GetReport();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workLeft, "workLeft", 0f);
			Scribe_Values.Look(ref billStartTick, "billStartTick", 0);
			Scribe_Values.Look(ref ticksSpentDoingRecipeWork, "ticksSpentDoingRecipeWork", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			AddEndCondition(delegate
			{
				Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
				return (!(thing is Building) || thing.Spawned) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(delegate
			{
				IBillGiver billGiver = job.GetTarget(TargetIndex.A).Thing as IBillGiver;
				if (billGiver != null)
				{
					if (job.bill.DeletedOrDereferenced)
					{
						return true;
					}
					if (!billGiver.CurrentlyUsableForBills())
					{
						return true;
					}
				}
				return false;
			});
			Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (job.targetQueueB != null && job.targetQueueB.Count == 1)
				{
					UnfinishedThing unfinishedThing = job.targetQueueB[0].Thing as UnfinishedThing;
					if (unfinishedThing != null)
					{
						unfinishedThing.BoundBill = (Bill_ProductionWithUft)job.bill;
					}
				}
			};
			yield return toil;
			yield return Toils_Jump.JumpIf(gotoBillGiver, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
			Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
			yield return extract;
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return getToHaulTarget;
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: true, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true);
			yield return JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
			Toil findPlaceTarget2 = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
			yield return findPlaceTarget2;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget2, storageMode: false);
			yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extract);
			yield return gotoBillGiver;
			yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
			yield return Toils_Recipe.DoRecipeWork().FailOnDespawnedNullOrForbiddenPlacedThings().FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_Recipe.FinishRecipeAndStartStoringProduct();
			if (job.RecipeDef.products.NullOrEmpty() && job.RecipeDef.specialProducts.NullOrEmpty())
			{
				yield break;
			}
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			findPlaceTarget2 = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return findPlaceTarget2;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, findPlaceTarget2, storageMode: true, tryStoreInSameStorageIfSpotCantHoldWholeStack: true);
			Toil recount = new Toil();
			recount.initAction = delegate
			{
				Bill_Production bill_Production = recount.actor.jobs.curJob.bill as Bill_Production;
				if (bill_Production != null && bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
				{
					base.Map.resourceCounter.UpdateResourceCounts();
				}
			};
			yield return recount;
		}

		private static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(string.Concat("JumpToAlsoCollectTargetInQueue run on ", actor, " who is not carrying something."));
				}
				else if (!actor.carryTracker.Full)
				{
					Job curJob = actor.jobs.curJob;
					List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
					if (!targetQueue.NullOrEmpty())
					{
						for (int i = 0; i < targetQueue.Count; i++)
						{
							if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing) && targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing) && !((float)(actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared > 64f))
							{
								int num = ((actor.carryTracker.CarriedThing != null) ? actor.carryTracker.CarriedThing.stackCount : 0);
								int a = curJob.countQueue[i];
								a = Mathf.Min(a, targetQueue[i].Thing.def.stackLimit - num);
								a = Mathf.Min(a, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
								if (a > 0)
								{
									curJob.count = a;
									curJob.SetTarget(ind, targetQueue[i].Thing);
									curJob.countQueue[i] -= a;
									if (curJob.countQueue[i] <= 0)
									{
										curJob.countQueue.RemoveAt(i);
										targetQueue.RemoveAt(i);
									}
									actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
									break;
								}
							}
						}
					}
				}
			};
			return toil;
		}
	}
}
