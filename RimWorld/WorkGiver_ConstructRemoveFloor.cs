using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructRemoveFloor : WorkGiver_ConstructAffectFloor
	{
		protected override DesignationDef DesDef => DesignationDefOf.RemoveFloor;

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.RemoveFloor, c);
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (!base.HasJobOnCell(pawn, c, forced: false))
			{
				return false;
			}
			if (!pawn.Map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				return false;
			}
			if (AnyBuildingBlockingFloorRemoval(c, pawn.Map))
			{
				return false;
			}
			return true;
		}

		public static bool AnyBuildingBlockingFloorRemoval(IntVec3 c, Map map)
		{
			if (!map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				return false;
			}
			Building firstBuilding = c.GetFirstBuilding(map);
			if (firstBuilding != null && firstBuilding.def.terrainAffordanceNeeded != null)
			{
				return !map.terrainGrid.UnderTerrainAt(c).affordances.Contains(firstBuilding.def.terrainAffordanceNeeded);
			}
			return false;
		}
	}
}
