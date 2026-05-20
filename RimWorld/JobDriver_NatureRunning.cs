using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_NatureRunning : JobDriver
{
	private static readonly IntRange WaitTicksRange = new IntRange(300, 600);

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	private Toil FindInterestingThing()
	{
		Toil toil = ToilMaker.MakeToil("FindInterestingThing");
		toil.initAction = delegate
		{
			if (NatureRunningUtility.TryFindNatureInterestTarget(pawn, out var interestTarget))
			{
				job.SetTarget(TargetIndex.A, interestTarget);
			}
			else
			{
				EndJobWith(JobCondition.Incompletable);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnChildLearningConditions();
		Toil findInterestingThing = FindInterestingThing();
		yield return findInterestingThing;
		Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
		toil.tickIntervalAction = delegate(int delta)
		{
			LearningUtility.LearningTickCheckEnd(pawn, delta);
		};
		yield return toil;
		Toil wait = ToilMaker.MakeToil("MakeNewToils");
		wait.initAction = delegate
		{
			wait.actor.pather.StopDead();
		};
		wait.tickIntervalAction = delegate(int delta)
		{
			if (!LearningUtility.LearningTickCheckEnd(pawn, delta))
			{
				pawn.rotationTracker.FaceTarget(base.TargetA);
			}
		};
		wait.defaultCompleteMode = ToilCompleteMode.Delay;
		wait.defaultDuration = WaitTicksRange.RandomInRange;
		wait.handlingFacing = true;
		yield return wait;
		yield return Toils_Jump.Jump(findInterestingThing);
	}
}
