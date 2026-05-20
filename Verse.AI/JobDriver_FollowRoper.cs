using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public class JobDriver_FollowRoper : JobDriver
{
	private const TargetIndex RoperInd = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate
		{
			Pawn pawn = job.GetTarget(TargetIndex.A).Thing as Pawn;
			if (!base.pawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				IntVec3 intVec = IntVec3.Invalid;
				if (pawn.jobs.curDriver is JobDriver_RopeToDestination)
				{
					intVec = pawn.CurJob.GetTarget(TargetIndex.B).Cell;
				}
				if (intVec.IsValid && pawn.Position == intVec)
				{
					GotoStandCell(intVec);
				}
				else
				{
					FollowRoper(pawn);
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		yield return toil;
	}

	private void FollowRoper(Pawn roper)
	{
		LocalTargetInfo localTargetInfo = new LocalTargetInfo(roper);
		PathEndMode peMode = PathEndMode.Touch;
		if (roper.Position.InHorDistOf(pawn.Position, 7.2f) && roper.pather.Moving && roper.pather.curPath != null)
		{
			int marchingPos = Mathf.Abs(roper.roping.Ropees.IndexOf(pawn));
			IntVec3 intVec = MarchingOrder(roper, marchingPos);
			if (intVec.IsValid)
			{
				localTargetInfo = intVec;
				peMode = PathEndMode.OnCell;
			}
		}
		if (!pawn.pather.Moving || pawn.pather.Destination != localTargetInfo)
		{
			pawn.pather.StartPath(localTargetInfo, peMode);
		}
	}

	private IntVec3 MarchingOrder(Pawn roper, int marchingPos)
	{
		PawnPath curPath = roper.pather.curPath;
		if (curPath.NodesLeftCount <= 0)
		{
			return IntVec3.Invalid;
		}
		Map map = pawn.Map;
		int value = -2 - marchingPos % 4;
		value = Mathf.Clamp(value, -curPath.NodesConsumedCount, curPath.NodesLeftCount);
		IntVec3 intVec = curPath.Peek(value);
		int num = Mathf.Abs(pawn.HashOffset()) % 3;
		if (value + 1 < curPath.NodesLeftCount && num != 0)
		{
			IntVec3 orig = curPath.Peek(value + 1) - intVec;
			IntVec3 intVec2 = ((num == 1) ? (intVec + orig.RotatedBy(Rot4.East)) : (intVec + orig.RotatedBy(Rot4.West)));
			if (intVec2.InBounds(map) && intVec2.Standable(map) && intVec2.GetDistrict(map) == intVec.GetDistrict(map))
			{
				intVec = intVec2;
			}
		}
		return intVec;
	}

	private void GotoStandCell(IntVec3 standCell)
	{
		if (!pawn.pather.Moving || pawn.pather.Destination != standCell)
		{
			pawn.pather.StartPath(standCell, PathEndMode.OnCell);
		}
	}

	public override bool IsContinuation(Job j)
	{
		return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
	}
}
