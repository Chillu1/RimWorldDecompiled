using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_GotoAndStandSociallyActive : JobDriver
{
	public virtual Toil StandToil
	{
		get
		{
			Toil toil = ToilMaker.MakeToil("StandToil");
			toil.tickIntervalAction = delegate(int delta)
			{
				if (!job.forceMaintainFacing)
				{
					Pawn pawn = JobDriver_StandAndBeSociallyActive.FindClosePawn(base.pawn);
					if (pawn != null)
					{
						base.pawn.rotationTracker.FaceCell(pawn.Position);
					}
				}
				base.pawn.GainComfortFromCellIfPossible(delta);
			};
			toil.socialMode = RandomSocialMode.SuperActive;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.handlingFacing = true;
			return toil;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA.Cell, job);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == base.TargetA.Cell)
			{
				pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
		yield return StandToil;
	}
}
