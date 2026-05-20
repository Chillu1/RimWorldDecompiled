using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_CarryToBiosculpterPod : JobDriver
{
	private const TargetIndex TakeeInd = TargetIndex.A;

	private const TargetIndex IngredientInd = TargetIndex.B;

	private const TargetIndex PodInd = TargetIndex.C;

	private List<Thing> pickedUpIngredients = new List<Thing>();

	private List<ThingCount> thingsToTransfer = new List<ThingCount>();

	private Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	private CompBiosculpterPod Pod => job.GetTarget(TargetIndex.C).Thing.TryGetComp<CompBiosculpterPod>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed) || !pawn.Reserve(Pod.parent, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		List<LocalTargetInfo> targetQueue = job.GetTargetQueue(TargetIndex.B);
		for (int i = 0; i < targetQueue.Count; i++)
		{
			if (!pawn.Reserve(targetQueue[i], job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckIdeology("Biosculpting"))
		{
			yield break;
		}
		AddFinishAction(delegate
		{
			if (Pod != null)
			{
				if (Pod.queuedEnterJob == job)
				{
					Pod.ClearQueuedInformation();
				}
				if (Pod.Occupant != Takee)
				{
					foreach (Thing pickedUpIngredient in pickedUpIngredients)
					{
						Thing lastResultingThing;
						if (pawn.inventory.Contains(pickedUpIngredient))
						{
							pawn.inventory.innerContainer.TryDrop(pickedUpIngredient, ThingPlaceMode.Near, out lastResultingThing);
						}
						else if (Takee.inventory.Contains(pickedUpIngredient))
						{
							Takee.inventory.innerContainer.TryDrop(pickedUpIngredient, ThingPlaceMode.Near, out lastResultingThing);
						}
					}
				}
			}
		});
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnDestroyedOrNull(TargetIndex.C);
		this.FailOnAggroMentalState(TargetIndex.A);
		this.FailOn(() => job.biosculpterCycleKey == null || !Pod.CanAcceptOnceCycleChosen(Takee));
		Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.C)
			.FailOn(() => Takee.IsFreeColonist && !Takee.Downed)
			.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil startCarryingTakee = Toils_Haul.StartCarryThing(TargetIndex.A);
		Toil goToThing = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
		Toil toil = Toils_Jump.JumpIf(goToThing, () => pawn.IsCarryingPawn(Takee) && job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
		Toil jumpIfGoToTakee = Toils_Jump.JumpIf(goToTakee, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
		yield return toil;
		yield return jumpIfGoToTakee;
		yield return DropCarryToGrabIngredients();
		foreach (Toil item in JobDriver_EnterBiosculpterPod.CollectIngredientsToilsHelper(TargetIndex.B, pawn, pickedUpIngredients))
		{
			yield return item;
		}
		yield return goToTakee;
		yield return TransferIngredientsAndPrepareCarryDownedPawn();
		yield return startCarryingTakee;
		yield return goToThing.FailOn(() => !Pod.PawnCarryingExtraCycleIngredients(Takee, job.biosculpterCycleKey));
		yield return JobDriver_EnterBiosculpterPod.PrepareToEnterToil(TargetIndex.C);
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			Pod.TryAcceptPawn(Takee, job.biosculpterCycleKey);
		};
		toil2.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil2;
	}

	private Toil TransferIngredientsAndPrepareCarryDownedPawn()
	{
		Toil toil = ToilMaker.MakeToil("TransferIngredientsAndPrepareCarryDownedPawn");
		toil.initAction = delegate
		{
			List<ThingDefCountClass> extraRequiredIngredients = Pod.GetCycle(job.biosculpterCycleKey).Props.extraRequiredIngredients;
			if (extraRequiredIngredients != null && !Pod.devFillPodLatch)
			{
				ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
				foreach (ThingDefCountClass item in extraRequiredIngredients)
				{
					if (Takee.inventory.Count(item.thingDef) < item.count)
					{
						if (pawn.inventory.Count(item.thingDef) < item.count)
						{
							EndJobWith(JobCondition.Incompletable);
							return;
						}
						thingsToTransfer.Clear();
						int num = 0;
						foreach (Thing item2 in innerContainer)
						{
							if (num >= item.count)
							{
								break;
							}
							if (item2.def == item.thingDef)
							{
								int num2 = Mathf.Min(item.count - num, item2.stackCount);
								thingsToTransfer.Add(new ThingCount(item2, Mathf.Min(item.count - num, item2.stackCount)));
								num += num2;
							}
						}
						foreach (ThingCount item3 in thingsToTransfer)
						{
							int num3 = Takee.inventory.innerContainer.TryAddOrTransfer(item3.Thing, item3.Count);
							if (num3 != item3.Count)
							{
								Log.Warning($"Only able to transfer x{num3} of the expected x{item3.Count} of {item3.Thing.Label} while CarryToBiosculpter");
								EndJobWith(JobCondition.Incompletable);
								return;
							}
						}
					}
				}
			}
			job.count = 1;
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	private Toil DropCarryToGrabIngredients()
	{
		Toil toil = ToilMaker.MakeToil("DropCarryToGrabIngredients");
		toil.initAction = delegate
		{
			if (pawn.carryTracker.CarriedThing != null)
			{
				pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	public override string GetReport()
	{
		if (!Pod.PawnCarryingExtraCycleIngredients(Takee, job.biosculpterCycleKey) && !Pod.PawnCarryingExtraCycleIngredients(pawn, job.biosculpterCycleKey))
		{
			return "BiosculpterJobReportCollectIngredients".Translate();
		}
		return base.GetReport();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pickedUpIngredients, "pickedUpIngredients", LookMode.Reference);
	}
}
