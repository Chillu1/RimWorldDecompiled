using System;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_WanderInGatheringArea : JobGiver_Wander
{
	public bool allowUnroofed = true;

	public float desiredRadius = -1f;

	protected override IntVec3 GetExactWanderDest(Pawn pawn)
	{
		Predicate<IntVec3> cellValidator = (IntVec3 x) => allowUnroofed || !x.Roofed(pawn.Map);
		if (desiredRadius > 0f && GatheringsUtility.TryFindRandomCellInGatheringAreaWithRadius(pawn, desiredRadius, cellValidator, out var result))
		{
			return result;
		}
		if (GatheringsUtility.TryFindRandomCellInGatheringArea(pawn, cellValidator, out result))
		{
			return result;
		}
		return IntVec3.Invalid;
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		throw new NotImplementedException();
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_WanderInGatheringArea obj = (JobGiver_WanderInGatheringArea)base.DeepCopy(resolve);
		obj.allowUnroofed = allowUnroofed;
		obj.desiredRadius = desiredRadius;
		return obj;
	}
}
