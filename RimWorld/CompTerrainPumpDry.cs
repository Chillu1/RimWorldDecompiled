using Verse;

namespace RimWorld
{
	public class CompTerrainPumpDry : CompTerrainPump
	{
		protected override void AffectCell(IntVec3 c)
		{
			AffectCell(parent.Map, c);
		}

		public static void AffectCell(Map map, IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain(map);
			TerrainDef terrainToDryTo = GetTerrainToDryTo(map, terrain);
			if (terrainToDryTo != null)
			{
				map.terrainGrid.SetTerrain(c, terrainToDryTo);
			}
			TerrainDef terrainDef = map.terrainGrid.UnderTerrainAt(c);
			if (terrainDef != null)
			{
				TerrainDef terrainToDryTo2 = GetTerrainToDryTo(map, terrainDef);
				if (terrainToDryTo2 != null)
				{
					map.terrainGrid.SetUnderTerrain(c, terrainToDryTo2);
				}
			}
		}

		private static TerrainDef GetTerrainToDryTo(Map map, TerrainDef terrainDef)
		{
			if (terrainDef.driesTo == null)
			{
				return null;
			}
			if (map.Biome == BiomeDefOf.SeaIce)
			{
				return TerrainDefOf.Ice;
			}
			return terrainDef.driesTo;
		}
	}
}
