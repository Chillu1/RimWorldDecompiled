using System.Collections.Generic;

namespace RimWorld.Planet;

public class WorldGenData : WorldComponent
{
	public Dictionary<PlanetLayer, List<PlanetTile>> roadNodes = new Dictionary<PlanetLayer, List<PlanetTile>>();

	public Dictionary<PlanetLayer, List<PlanetTile>> ancientSites = new Dictionary<PlanetLayer, List<PlanetTile>>();

	public WorldGenData(World world)
		: base(world)
	{
	}
}
