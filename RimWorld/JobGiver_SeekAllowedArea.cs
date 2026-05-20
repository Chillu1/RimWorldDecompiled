using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SeekAllowedArea : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.Position.IsForbidden(pawn))
		{
			return null;
		}
		if (HasJobWithSpawnedAllowedTarget(pawn))
		{
			return null;
		}
		Region region = pawn.GetRegion();
		if (region == null)
		{
			return null;
		}
		TraverseParms traverseParms = TraverseParms.For(pawn);
		RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, isDestination: false);
		Region reg = null;
		RegionProcessor regionProcessor = delegate(Region r)
		{
			if (r.IsDoorway)
			{
				return false;
			}
			if (!r.IsForbiddenEntirely(pawn))
			{
				reg = r;
				return true;
			}
			return false;
		};
		RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999);
		if (reg != null)
		{
			if (!reg.TryFindRandomCellInRegionUnforbidden(pawn, null, out var result))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Goto, result);
		}
		return null;
	}

	private bool HasJobWithSpawnedAllowedTarget(Pawn pawn)
	{
		Job curJob = pawn.CurJob;
		if (curJob == null)
		{
			return false;
		}
		if (!IsSpawnedAllowedTarget(curJob.targetA, pawn) && !IsSpawnedAllowedTarget(curJob.targetB, pawn))
		{
			return IsSpawnedAllowedTarget(curJob.targetC, pawn);
		}
		return true;
	}

	private bool IsSpawnedAllowedTarget(LocalTargetInfo target, Pawn pawn)
	{
		if (!target.IsValid)
		{
			return false;
		}
		if (target.HasThing)
		{
			if (target.Thing.Spawned)
			{
				return !target.Thing.Position.IsForbidden(pawn);
			}
			return false;
		}
		if (target.Cell.InBounds(pawn.Map))
		{
			return !target.Cell.IsForbidden(pawn);
		}
		return false;
	}
}
