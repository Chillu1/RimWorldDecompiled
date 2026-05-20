using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_GiveToPawn : JobDriver
{
	private const TargetIndex ItemInd = TargetIndex.A;

	private const TargetIndex PawnInd = TargetIndex.B;

	private Thing Item => job.GetTarget(TargetIndex.A).Thing;

	private Pawn Receiver => job.GetTarget(TargetIndex.B).Pawn;

	public int CountBeingHauled
	{
		get
		{
			if (pawn.carryTracker.CarriedThing == null)
			{
				return 0;
			}
			return pawn.carryTracker.CarriedThing.stackCount;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
		this.FailOn(() => !GiveItemsToPawnUtility.IsWaitingForItems(Receiver));
		Toil setItemTarget = SetItemTarget();
		yield return setItemTarget;
		Toil reserve = Toils_Reserve.Reserve(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A);
		yield return reserve;
		yield return DetermineNumToHaul();
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		Toil checkForDuplicates = Toils_Haul.CheckForGetOpportunityDuplicate(reserve, TargetIndex.A, TargetIndex.None, takeFromValidStorage: true, (Thing x) => x.def == Item.def);
		Toil haulToContainer = Toils_Haul.CarryHauledThingToContainer();
		yield return Toils_Jump.JumpIf(haulToContainer, () => GiveItemsToPawnUtility.ItemCountLeftToCollect(Receiver) <= 0);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		yield return Toils_Jump.JumpIf(haulToContainer, () => GiveItemsToPawnUtility.ItemCountLeftToCollect(Receiver) <= 0);
		yield return checkForDuplicates;
		yield return haulToContainer;
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.B);
		yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A);
		yield return Toils_Jump.JumpIf(setItemTarget, () => GiveItemsToPawnUtility.ItemCountLeftToCollect(Receiver) > 0);
		yield return Toils_General.Do(delegate
		{
			QuestUtility.SendQuestTargetSignals(Receiver.questTags, "ReceivedItems", pawn.Named("GIVER"), Receiver.Named("RECEIVER"));
		});
	}

	private Toil DetermineNumToHaul()
	{
		Toil toil = ToilMaker.MakeToil("DetermineNumToHaul");
		toil.initAction = delegate
		{
			int num = GiveItemsToPawnUtility.ItemCountLeftToCollect(Receiver);
			if (num <= 0)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
			}
			else
			{
				job.count = num;
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	private Toil SetItemTarget()
	{
		Toil toil = ToilMaker.MakeToil("SetItemTarget");
		toil.initAction = delegate
		{
			Thing thing = GiveItemsToPawnUtility.FindItemToGive(pawn, Item.def);
			if (thing == null)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			else
			{
				job.SetTarget(TargetIndex.A, thing);
			}
		};
		return toil;
	}
}
