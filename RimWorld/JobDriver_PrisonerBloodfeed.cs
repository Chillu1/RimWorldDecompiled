using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PrisonerBloodfeed : JobDriver
{
	public const float BloodLoss = 0.4499f;

	public const int WaitTicks = 120;

	private const float HemogenGain = 0.2f;

	private const float NutritionGain = 0.1f;

	protected Pawn Prisoner => (Pawn)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure || Prisoner.InAggroMentalState || Prisoner.guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.Bloodfeed));
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, PrisonerInteractionModeDefOf.Bloodfeed);
		yield return Toils_General.WaitWith(TargetIndex.A, 120, useProgressBar: true).PlaySustainerOrSound(SoundDefOf.Bloodfeed_Cast);
		yield return Toils_General.Do(delegate
		{
			SanguophageUtility.DoBite(pawn, Prisoner, 0.2f, 0.1f, 0.4499f, 1f, IntRange.One, ThoughtDefOf.FedOn, ThoughtDefOf.FedOn_Social);
		});
		yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
	}
}
