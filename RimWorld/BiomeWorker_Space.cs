using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_Space : BiomeWorker
{
	public override bool CanPlaceOnLayer(BiomeDef biome, PlanetLayer layer)
	{
		return layer.Def == PlanetLayerDefOf.Orbit;
	}

	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		return -100f;
	}
}
