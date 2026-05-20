using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ChatWithPrisoner : JobDriver
{
	protected Pawn Talkee => (Pawn)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override string ReportStringProcessed(string str)
	{
		if (Talkee.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.ReduceResistance))
		{
			return "JobReport_ReduceResistance".Translate(Talkee);
		}
		return base.ReportStringProcessed(str);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOnMentalState(TargetIndex.A);
		this.FailOnNotAwake(TargetIndex.A);
		this.FailOn(() => !Talkee.IsPrisonerOfColony || !Talkee.guest.PrisonerIsSecure);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A).FailOn(() => !Talkee.guest.ScheduledForInteraction);
		yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.TryRecruit(TargetIndex.A);
	}
}
