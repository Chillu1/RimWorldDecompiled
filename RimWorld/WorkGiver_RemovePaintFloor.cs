using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_RemovePaintFloor : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.RemovePaintFloor);
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.RemovePaintFloor))
			{
				yield return item.target.Cell;
			}
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (!pawn.Map.terrainGrid.TerrainAt(c).isPaintable)
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.RemovePaintFloor) == null)
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.PaintFloor) != null)
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor) != null)
			{
				return false;
			}
			Building firstBuilding = c.GetFirstBuilding(pawn.Map);
			TerrainDef terrainDef = pawn.Map.terrainGrid.UnderTerrainAt(c);
			if (firstBuilding != null && firstBuilding.def.terrainAffordanceNeeded != null && terrainDef != null && !terrainDef.affordances.Contains(firstBuilding.def.terrainAffordanceNeeded))
			{
				return false;
			}
			if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Floor, forced))
			{
				return false;
			}
			return true;
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
		{
			Job job = JobMaker.MakeJob(JobDefOf.RemovePaintFloor);
			job.AddQueuedTarget(TargetIndex.A, cell);
			for (int i = 0; i < 100; i++)
			{
				IntVec3 intVec = cell + GenRadial.RadialPattern[i];
				if (intVec.InBounds(pawn.Map) && !intVec.Fogged(pawn.Map) && pawn.CanReach(intVec, PathEndMode.Touch, Danger.Deadly) && !job.targetQueueA.Contains(intVec))
				{
					job.AddQueuedTarget(TargetIndex.A, intVec);
					if (job.GetTargetQueue(TargetIndex.A).Count >= 10)
					{
						break;
					}
				}
			}
			if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
			{
				job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
			}
			return job;
		}
	}
}
