using Verse;

namespace RimWorld
{
	public class RoadDefGenStep_DryWithFallback : RoadDefGenStep
	{
		public TerrainDef fallback;

		public override void Place(Map map, IntVec3 position, TerrainDef rockDef, IntVec3 origin, GenStep_Roads.DistanceElement[,] distance)
		{
			PlaceWorker(map, position, fallback);
		}

		public static void PlaceWorker(Map map, IntVec3 position, TerrainDef fallback)
		{
			while (map.terrainGrid.TerrainAt(position).driesTo != null)
			{
				map.terrainGrid.SetTerrain(position, map.terrainGrid.TerrainAt(position).driesTo);
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(position);
			if (terrainDef.passability == Traversability.Impassable || terrainDef.IsRiver)
			{
				map.terrainGrid.SetTerrain(position, fallback);
			}
		}
	}
}
