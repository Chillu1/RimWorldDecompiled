using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_JumpInWater : ThinkNode_JobGiver
{
	private const float ActivateChance = 1f;

	private readonly IntRange MaxDistance = new IntRange(10, 16);

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (Rand.Value < 1f && RCellFinder.TryFindRandomCellNearWith(pawn.Position, (IntVec3 pos) => pos.GetTerrain(pawn.Map).extinguishesFire, pawn.Map, out var result, 5, MaxDistance.RandomInRange))
		{
			return JobMaker.MakeJob(JobDefOf.Goto, result);
		}
		return null;
	}
}
