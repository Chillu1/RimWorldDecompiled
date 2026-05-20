using Verse;

namespace RimWorld.Planet;

public static class WorldReachabilityUtility
{
	public static bool CanReach(this Caravan c, PlanetTile tile)
	{
		return Find.WorldReachability.CanReach(c, tile);
	}
}
