using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

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

	public bool AnyIngredientsQueued => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty();

	public override string GetReport()
	{
		if (ModsConfig.BiotechActive && job.bill is Bill_Mech bill)
		{
			return MechanitorUtility.GetMechGestationJobString(this, pawn, bill);
		}
		if (job.RecipeDef != null)
		{
			return ReportStringProcessed(job.RecipeDef.jobString);
		}
		return base.GetReport();
	}

	public override bool IsContinuation(Job j)
	{
		return j.bill == job.bill;
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
		Thing thing = job.GetTarget(TargetIndex.A).Thing;
		if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (thing != null && thing.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed))
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
			if (job.GetTarget(TargetIndex.A).Thing is IBillGiver billGiver)
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
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (job.targetQueueB != null && job.targetQueueB.Count == 1 && job.targetQueueB[0].Thing is UnfinishedThing { Destroyed: false } unfinishedThing)
			{
				unfinishedThing.BoundBill = (Bill_ProductionWithUft)job.bill;
			}
			job.bill.Notify_DoBillStarted(pawn);
		};
		yield return toil;
		yield return Toils_Jump.JumpIf(gotoBillGiver, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
		foreach (Toil item in CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true, BillGiver is Building_WorkTableAutonomous))
		{
			yield return item;
		}
		yield return gotoBillGiver;
		yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
		yield return Toils_Recipe.DoRecipeWork().FailOnDespawnedNullOrForbiddenPlacedThings(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		yield return Toils_Recipe.CheckIfRecipeCanFinishNow();
		yield return Toils_Recipe.FinishRecipeAndStartStoringProduct(TargetIndex.None);
	}

	public static IEnumerable<Toil> CollectIngredientsToils(TargetIndex ingredientInd, TargetIndex billGiverInd, TargetIndex ingredientPlaceCellInd, bool subtractNumTakenFromJobCount = false, bool failIfStackCountLessThanJobCount = true, bool placeInBillGiver = false)
	{
		Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientInd);
		yield return extract;
		Toil jumpIfHaveTargetInQueue = Toils_Jump.JumpIfHaveTargetInQueue(ingredientInd, extract);
		yield return JumpIfTargetInsideBillGiver(jumpIfHaveTargetInQueue, ingredientInd, billGiverInd);
		Toil getToHaulTarget = Toils_Goto.GotoThing(ingredientInd, PathEndMode.ClosestTouch, canGotoSpawnedParent: true).FailOnForbidden(ingredientInd).FailOnSomeonePhysicallyInteracting(ingredientInd);
		yield return getToHaulTarget;
		yield return Toils_Haul.StartCarryThing(ingredientInd, putRemainderInQueue: true, subtractNumTakenFromJobCount, failIfStackCountLessThanJobCount, reserve: false, canTakeFromInventory: true);
		yield return JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
		yield return Toils_Goto.GotoThing(billGiverInd, PathEndMode.InteractionCell).FailOnDestroyedOrNull(ingredientInd);
		if (!placeInBillGiver)
		{
			Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(billGiverInd, ingredientInd, ingredientPlaceCellInd);
			yield return findPlaceTarget;
			yield return Toils_Haul.PlaceHauledThingInCell(ingredientPlaceCellInd, findPlaceTarget, storageMode: false);
			Toil physReserveToil = ToilMaker.MakeToil("CollectIngredientsToils");
			physReserveToil.initAction = delegate
			{
				physReserveToil.actor.Map.physicalInteractionReservationManager.Reserve(physReserveToil.actor, physReserveToil.actor.CurJob, physReserveToil.actor.CurJob.GetTarget(ingredientInd));
			};
			yield return physReserveToil;
		}
		else
		{
			yield return Toils_Haul.DepositHauledThingInContainer(billGiverInd, ingredientInd);
		}
		yield return jumpIfHaveTargetInQueue;
	}

	private static Toil JumpIfTargetInsideBillGiver(Toil jumpToil, TargetIndex ingredient, TargetIndex billGiver)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfTargetInsideBillGiver");
		toil.initAction = delegate
		{
			Thing thing = toil.actor.CurJob.GetTarget(billGiver).Thing;
			if (thing != null && thing.Spawned)
			{
				Thing thing2 = toil.actor.jobs.curJob.GetTarget(ingredient).Thing;
				if (thing2 != null)
				{
					ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
					if (thingOwner != null && thingOwner.Contains(thing2))
					{
						HaulAIUtility.UpdateJobWithPlacedThings(toil.actor.jobs.curJob, thing2, thing2.stackCount);
						toil.actor.jobs.curDriver.JumpToToil(jumpToil);
					}
				}
			}
		};
		return toil;
	}

	public static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("JumpToCollectNextIntoHandsForBill");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			if (actor.carryTracker.CarriedThing == null)
			{
				Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor?.ToString() + " who is not carrying something.");
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
