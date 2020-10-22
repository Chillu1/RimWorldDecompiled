using System.Collections.Generic;
using RimWorld.Planet;

namespace Verse.AI
{
	public class JobDriver_Goto : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			toil.AddPreTickAction(delegate
			{
				if (job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position))
				{
					TryExitMap();
				}
			});
			toil.FailOn(() => job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn));
			yield return toil;
			Toil toil2 = new Toil();
			toil2.initAction = delegate
			{
				if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == base.TargetA.Cell)
				{
					pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
				}
				if (job.exitMapOnArrival && (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position)))
				{
					TryExitMap();
				}
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}

		private void TryExitMap()
		{
			if (!job.failIfCantJoinOrCreateCaravan || CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn))
			{
				pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
			}
		}
	}
}
