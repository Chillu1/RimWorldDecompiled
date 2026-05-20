using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Spectate : JobDriver
{
	private const TargetIndex MySpotOrChairInd = TargetIndex.A;

	private const TargetIndex WatchTargetInd = TargetIndex.B;

	private const TargetIndex ChairInd = TargetIndex.C;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.ReserveSittableOrSpot(job.GetTarget(TargetIndex.A).Cell, job, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (job.GetTarget(TargetIndex.C).HasThing)
		{
			this.EndOnDespawnedOrNull(TargetIndex.C);
		}
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceCell(job.GetTarget(TargetIndex.B).Cell);
			pawn.GainComfortFromCellIfPossible(delta);
			if (pawn.IsHashIntervalTick(100, delta))
			{
				pawn.jobs.CheckForJobOverride();
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.handlingFacing = true;
		yield return toil;
	}
}
