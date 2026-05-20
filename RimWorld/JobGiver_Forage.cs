using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Forage : ThinkNode_JobGiver
{
	private const int SearchRadius = 30;

	private const float MinFertility = 0.7f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 30, delegate(IntVec3 c)
		{
			if (c.GetFertility(pawn.Map) < 0.7f)
			{
				return false;
			}
			if (!c.GetTerrain(pawn.Map).natural)
			{
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				return false;
			}
			return pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.Deadly) ? true : false;
		}, out var result))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.Forage, result);
	}
}
