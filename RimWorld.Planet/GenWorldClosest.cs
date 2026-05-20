using System;
using Verse;

namespace RimWorld.Planet;

public static class GenWorldClosest
{
	public static bool TryFindClosestTile(PlanetTile rootTile, Predicate<PlanetTile> predicate, out PlanetTile foundTile, int maxTilesToScan = int.MaxValue, bool canSearchThroughImpassable = true)
	{
		PlanetTile foundTileLocal = PlanetTile.Invalid;
		rootTile.Layer.Filler.FloodFill(rootTile, (PlanetTile x) => canSearchThroughImpassable || !Find.World.Impassable(x), delegate(PlanetTile t)
		{
			bool num = predicate(t);
			if (num)
			{
				foundTileLocal = t;
			}
			return num;
		}, maxTilesToScan);
		foundTile = foundTileLocal;
		return foundTileLocal.Valid;
	}

	public static bool TryFindClosestPassableTile(PlanetTile rootTile, out PlanetTile foundTile)
	{
		return TryFindClosestTile(rootTile, (PlanetTile x) => !Find.World.Impassable(x), out foundTile);
	}
}
