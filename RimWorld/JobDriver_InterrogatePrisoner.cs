using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_InterrogatePrisoner : JobDriver
{
	private const int NumTalks = 3;

	protected Pawn Prisoner => (Pawn)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (ModsConfig.AnomalyActive)
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnMentalState(TargetIndex.A);
			this.FailOnNotAwake(TargetIndex.A);
			this.FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure);
			for (int i = 0; i < 3; i++)
			{
				yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, PrisonerInteractionModeDefOf.Interrogate);
				yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
				yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
				yield return Toils_Interpersonal.Interrogate(pawn, Prisoner);
			}
			yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
		}
	}
}
