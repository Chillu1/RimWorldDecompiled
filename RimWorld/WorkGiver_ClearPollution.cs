using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ClearPollution : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
	{
		return pawn.Map.areaManager.PollutionClear.ActiveCells;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return pawn.Map.areaManager.PollutionClear.TrueCount == 0;
	}

	public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Clear pollution"))
		{
			return false;
		}
		if (!pawn.Map.pollutionGrid.IsPolluted(c))
		{
			return false;
		}
		if (!pawn.CanReserve(c, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.ClearPollution, c);
	}
}
