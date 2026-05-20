using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Skydreaming : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnChildLearningConditions();
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			LearningUtility.LearningTickCheckEnd(pawn, delta);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = job.def.learningDuration;
		toil.FailOn(() => pawn.Position.Roofed(pawn.Map));
		yield return toil;
	}
}
