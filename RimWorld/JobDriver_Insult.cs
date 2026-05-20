using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Insult : JobDriver
{
	private const TargetIndex TargetInd = TargetIndex.A;

	private Pawn Target => (Pawn)(Thing)pawn.CurJob.GetTarget(TargetIndex.A);

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return InsultingSpreeDelayToil();
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		Toil toil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		toil.socialMode = RandomSocialMode.Off;
		yield return toil;
		yield return InteractToil();
	}

	private Toil InteractToil()
	{
		return Toils_General.Do(delegate
		{
			if (pawn.interactions.TryInteractWith(Target, InteractionDefOf.Insult) && pawn.MentalState is MentalState_InsultingSpree mentalState_InsultingSpree)
			{
				mentalState_InsultingSpree.lastInsultTicks = Find.TickManager.TicksGame;
				if (mentalState_InsultingSpree.target == Target)
				{
					mentalState_InsultingSpree.insultedTargetAtLeastOnce = true;
				}
			}
		});
	}

	private Toil InsultingSpreeDelayToil()
	{
		Toil toil = ToilMaker.MakeToil("InsultingSpreeDelayToil");
		toil.initAction = WaitAction;
		toil.tickIntervalAction = delegate
		{
			WaitAction();
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		return toil;
		void WaitAction()
		{
			if (!(pawn.MentalState is MentalState_InsultingSpree mentalState_InsultingSpree) || Find.TickManager.TicksGame - mentalState_InsultingSpree.lastInsultTicks >= 1200)
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		}
	}
}
