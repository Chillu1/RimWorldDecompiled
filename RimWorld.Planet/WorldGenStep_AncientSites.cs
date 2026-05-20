using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_AncientSites : WorldGenStep
{
	public FloatRange ancientSitesPer100kTiles;

	public override int SeedPart => 976238715;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		GenerateAncientSites(layer);
	}

	private void GenerateAncientSites(PlanetLayer layer)
	{
		int num = GenMath.RoundRandom((float)layer.TilesCount / 100000f * ancientSitesPer100kTiles.RandomInRange);
		Dictionary<PlanetLayer, List<PlanetTile>> ancientSites = Find.World.genData.ancientSites;
		if (!ancientSites.TryGetValue(layer, out var value))
		{
			value = (ancientSites[layer] = new List<PlanetTile>());
		}
		for (int i = 0; i < num; i++)
		{
			value.Add(TileFinder.RandomSettlementTileFor(layer, null));
		}
	}
}
