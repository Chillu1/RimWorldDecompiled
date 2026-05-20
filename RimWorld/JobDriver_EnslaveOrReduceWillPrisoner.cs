using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_EnslaveOrReduceWillPrisoner : JobDriver
{
	protected Pawn Prisoner => (Pawn)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOnMentalState(TargetIndex.A);
		this.FailOnNotAwake(TargetIndex.A);
		this.FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure);
		PrisonerInteractionModeDef interaction = (Prisoner.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Enslave) ? PrisonerInteractionModeDefOf.Enslave : PrisonerInteractionModeDefOf.ReduceWill);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, interaction);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ReduceWill(pawn, Prisoner);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, interaction);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ReduceWill(pawn, Prisoner);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, interaction);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A).FailOn(() => !Prisoner.guest.ScheduledForInteraction);
		yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
		yield return Toils_Interpersonal.TryEnslave(TargetIndex.A);
	}
}
