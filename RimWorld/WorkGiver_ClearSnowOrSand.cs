using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ClearSnowOrSand : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
	{
		return pawn.Map.areaManager.SnowOrSandClear.ActiveCells;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return pawn.Map.areaManager.SnowOrSandClear.TrueCount == 0;
	}

	public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		if (pawn.Map.snowGrid.GetDepth(c) < 0.2f && c.GetSandDepth(pawn.Map) < 0.2f)
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
		return JobMaker.MakeJob(JobDefOf.ClearSnow, c);
	}

	private bool IsSand(IntVec3 cell, Map map)
	{
		return cell.GetSandDepth(map) > map.snowGrid.GetDepth(cell);
	}

	public override string PostProcessedGerund(Job job)
	{
		if (IsSand(job.targetA.Cell, Find.CurrentMap))
		{
			return "ClearingSandGerund".Translate();
		}
		return base.PostProcessedGerund(job);
	}
}
