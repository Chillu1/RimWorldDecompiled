using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_GoForWalk : JoyGiver
{
	public override Job TryGiveJob(Pawn pawn)
	{
		if (!JoyUtility.EnjoyableOutsideNow(pawn))
		{
			return null;
		}
		if (PawnUtility.WillSoonHaveBasicNeed(pawn))
		{
			return null;
		}
		if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn), RegionValidator, 100, out var result))
		{
			return null;
		}
		if (!result.TryFindRandomCellInRegionUnforbidden(pawn, CellValidator, out var result2))
		{
			return null;
		}
		if (!WalkPathFinder.TryFindWalkPath(pawn, result2, out var result3))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(def.jobDef, result3[0]);
		job.targetQueueA = new List<LocalTargetInfo>();
		for (int i = 1; i < result3.Count; i++)
		{
			job.targetQueueA.Add(result3[i]);
		}
		job.locomotionUrgency = LocomotionUrgency.Walk;
		return job;
		bool CellValidator(IntVec3 x)
		{
			if (!PawnUtility.KnownDangerAt(x, pawn.Map, pawn) && !x.Fogged(pawn.Map) && !x.GetTerrain(pawn.Map).avoidWander && x.Standable(pawn.Map))
			{
				return !x.Roofed(pawn.Map);
			}
			return false;
		}
		bool RegionValidator(Region x)
		{
			IntVec3 result4;
			if (x.Room.PsychologicallyOutdoors && !x.IsForbiddenEntirely(pawn))
			{
				return x.TryFindRandomCellInRegionUnforbidden(pawn, CellValidator, out result4);
			}
			return false;
		}
	}
}
