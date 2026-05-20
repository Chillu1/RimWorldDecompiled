using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace Verse.AI;

public class JobDriver_Goto : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		LocalTargetInfo lookAtTarget = job.GetTarget(TargetIndex.B);
		Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		toil.AddPreTickAction(delegate
		{
			if (job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position))
			{
				TryExitMap();
			}
		});
		toil.FailOn(() => job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn));
		toil.FailOn(() => job.GetTarget(TargetIndex.A).Thing is Pawn pawn && pawn.ParentHolder is Corpse);
		toil.FailOn(() => job.GetTarget(TargetIndex.A).Thing?.Destroyed ?? false);
		if (lookAtTarget.IsValid)
		{
			toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate
			{
				pawn.rotationTracker.FaceCell(lookAtTarget.Cell);
			});
			toil.handlingFacing = true;
		}
		toil.AddFinishAction(delegate
		{
			if (job.controlGroupTag != null && job.controlGroupTag != null)
			{
				pawn.GetOverseer()?.mechanitor.GetControlGroup(pawn).SetTag(pawn, job.controlGroupTag);
			}
		});
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == base.TargetA.Cell)
			{
				pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
			}
			if (!job.ritualTag.NullOrEmpty() && pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual)
			{
				lordJob_Ritual.AddTagForPawn(pawn, job.ritualTag);
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
			if (ModsConfig.BiotechActive)
			{
				MechanitorUtility.Notify_PawnGotoLeftMap(pawn, pawn.Map);
			}
			if (!ModsConfig.AnomalyActive || MetalhorrorUtility.TryPawnExitMap(pawn))
			{
				pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
			}
		}
	}
}
