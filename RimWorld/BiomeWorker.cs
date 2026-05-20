using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class BiomeWorker
{
	public abstract float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile);

	public virtual bool CanPlaceOnLayer(BiomeDef biome, PlanetLayer layer)
	{
		if (!biome.layerWhitelist.NullOrEmpty() && !biome.layerWhitelist.Contains(layer.Def))
		{
			return false;
		}
		if (!biome.layerBlacklist.NullOrEmpty() && biome.layerBlacklist.Contains(layer.Def))
		{
			return false;
		}
		if (layer.Def.onlyAllowWhitelistedBiomes && (biome.layerWhitelist.NullOrEmpty() || !biome.layerWhitelist.Contains(layer.Def)))
		{
			return false;
		}
		return true;
	}
}
