using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_ReleasePrisoner : JobDriver
{
	private const TargetIndex PrisonerInd = TargetIndex.A;

	private const TargetIndex ReleaseCellInd = TargetIndex.B;

	private Pawn Prisoner => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Prisoner, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.B);
		this.FailOn(() => ((Pawn)(Thing)GetActor().CurJob.GetTarget(TargetIndex.A)).guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.Release));
		this.FailOnDowned(TargetIndex.A);
		this.FailOnAggroMentalState(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
		yield return carryToCell;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: false);
		Toil setReleased = ToilMaker.MakeToil("MakeNewToils");
		setReleased.initAction = delegate
		{
			Pawn pawn = setReleased.actor.jobs.curJob.targetA.Thing as Pawn;
			GenGuest.PrisonerRelease(pawn);
			if (!PawnBanishUtility.WouldBeLeftToDie(pawn, pawn.Map.Tile))
			{
				GenGuest.AddHealthyPrisonerReleasedThoughts(pawn);
			}
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "Released", pawn.Named("SUBJECT"));
		};
		yield return setReleased;
	}
}
