using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Flee : JobDriver
{
	protected const TargetIndex DestInd = TargetIndex.A;

	protected const TargetIndex DangerInd = TargetIndex.B;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.GetTarget(TargetIndex.A).Cell);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.atomicWithPrevious = true;
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.initAction = delegate
		{
			if (pawn.IsColonist)
			{
				MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistFleeing);
			}
		};
		yield return toil;
		Toil toil2 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		toil2.tickAction = delegate
		{
			if (job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position))
			{
				ExitMap();
			}
		};
		toil2.FailOn(() => pawn.Downed && !pawn.health.CanCrawl);
		yield return toil2;
		Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
		toil3.initAction = delegate
		{
			if (job.exitMapOnArrival && (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position)))
			{
				ExitMap();
			}
		};
		toil3.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil3;
	}

	private void ExitMap()
	{
		pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
	}
}
