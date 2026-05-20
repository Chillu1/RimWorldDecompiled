using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_CubeSculpting : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (RCellFinder.TryFindNearbyEmptyCell(pawn, out var cell, (IntVec3 c, Map map) => ValidateCell(c, map, pawn)))
		{
			return JobMaker.MakeJob(JobDefOf.BuildCubeSculpture, cell);
		}
		return null;
	}

	private static bool ValidateCell(IntVec3 cell, Map map, Pawn pawn)
	{
		foreach (IntVec3 item in GenAdjFast.AdjacentCellsCardinal(cell))
		{
			if (item.InBounds(map) && (item.GetEdifice(map) != null || cell.GetFirstThing<Blueprint>(map) != null))
			{
				return false;
			}
		}
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
		TerrainAffordanceDef terrainAffordanceNeed = ThingDefOf.DirtCubeSculpture.GetTerrainAffordanceNeed();
		if (terrainDef.IsRoad || !terrainDef.affordances.Contains(terrainAffordanceNeed))
		{
			return false;
		}
		Plant plant = cell.GetPlant(map);
		if (plant != null && plant.def.plant.IsTree)
		{
			return false;
		}
		if (!pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
		{
			return false;
		}
		if (cell.GetEdifice(map) == null)
		{
			return cell.GetFirstThing<Blueprint>(map) == null;
		}
		return false;
	}
}
