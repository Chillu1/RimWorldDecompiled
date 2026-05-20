using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FindOxygen : ThinkNode_JobGiver
{
	private static List<IntVec3> tmpWorkingCellList = new List<IntVec3>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return null;
		}
		if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
		{
			return null;
		}
		if (!pawn.ConcernedByVacuum)
		{
			return null;
		}
		if (!pawn.health.hediffSet.TryGetHediff(HediffDefOf.VacuumExposure, out var hediff) || hediff.CurStageIndex < 1)
		{
			return null;
		}
		float vacuum = pawn.Position.GetVacuum(pawn.Map);
		IntVec3 cell = GuessJobDestination(pawn);
		if (cell.IsValid)
		{
			vacuum = cell.GetVacuum(pawn.Map);
		}
		if (vacuum < 0.5f)
		{
			return null;
		}
		Region region = ClosestOxygenatedRegion(pawn.Position, pawn.MapHeld, pawn, TraverseParms.For(pawn));
		if (region != null)
		{
			TryGetAllowedCellInRegion(region, pawn, out var cell2);
			return JobMaker.MakeJob(JobDefOf.GotoOxygenatedArea, cell2);
		}
		return null;
	}

	private IntVec3 GuessJobDestination(Pawn pawn)
	{
		if (pawn.CurJob == null)
		{
			return IntVec3.Invalid;
		}
		Job curJob = pawn.CurJob;
		if (!curJob.targetA.HasThing && curJob.targetA.Cell.IsValid)
		{
			return curJob.targetA.Cell;
		}
		if (!curJob.targetB.HasThing && curJob.targetB.Cell.IsValid)
		{
			return curJob.targetB.Cell;
		}
		if (!curJob.targetC.HasThing && curJob.targetC.Cell.IsValid)
		{
			return curJob.targetC.Cell;
		}
		if (curJob.targetA.HasThing && curJob.targetA.Cell.IsValid)
		{
			return curJob.targetA.Cell;
		}
		if (curJob.targetB.HasThing && curJob.targetB.Cell.IsValid)
		{
			return curJob.targetB.Cell;
		}
		if (curJob.targetC.HasThing && curJob.targetC.Cell.IsValid)
		{
			return curJob.targetC.Cell;
		}
		return IntVec3.Invalid;
	}

	private static Region ClosestOxygenatedRegion(IntVec3 root, Map map, Pawn pawn, TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
	{
		Region region = root.GetRegion(map, traversableRegionTypes);
		if (region == null)
		{
			return null;
		}
		RegionEntryPredicate entryCondition = (Region _, Region r) => r.Allows(traverseParms, isDestination: false);
		Region foundReg = null;
		RegionProcessor regionProcessor = delegate(Region r)
		{
			if (r.IsDoorway)
			{
				return false;
			}
			if (!TryGetAllowedCellInRegion(r, pawn, out var _))
			{
				return false;
			}
			if (r.Room.Vacuum < 0.5f)
			{
				foundReg = r;
				return true;
			}
			return false;
		};
		RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, traversableRegionTypes);
		return foundReg;
	}

	private static bool TryGetAllowedCellInRegion(Region region, Pawn pawn, out IntVec3 cell, float maxRadius = -1f)
	{
		cell = IntVec3.Invalid;
		for (int i = 0; i < 100; i++)
		{
			IntVec3 randomCell = region.RandomCell;
			if (!randomCell.Fogged(pawn.Map) && randomCell.InAllowedArea(pawn) && (maxRadius < 0f || randomCell.InHorDistOf(pawn.Position, maxRadius)))
			{
				cell = randomCell;
				return true;
			}
		}
		foreach (IntVec3 item in region.Cells.InRandomOrder(tmpWorkingCellList))
		{
			if (!item.Fogged(pawn.Map) && item.InAllowedArea(pawn) && (maxRadius < 0f || item.InHorDistOf(pawn.Position, maxRadius)))
			{
				cell = item;
				return true;
			}
		}
		return false;
	}
}
