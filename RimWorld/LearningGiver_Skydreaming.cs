using Verse;
using Verse.AI;

namespace RimWorld;

public class LearningGiver_Skydreaming : LearningGiver
{
	public override bool CanDo(Pawn pawn)
	{
		if (!base.CanDo(pawn))
		{
			return false;
		}
		IntVec3 result;
		return RCellFinder.TryFindAllowedUnroofedSpotOutsideColony(pawn.Position, pawn, out result);
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (!RCellFinder.TryFindAllowedUnroofedSpotOutsideColony(pawn.Position, pawn, out var result))
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, result);
	}
}
