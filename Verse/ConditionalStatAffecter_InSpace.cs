using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class ConditionalStatAffecter_InSpace : ConditionalStatAffecter
{
	public override string Label => "StatsReport_InSpace".Translate();

	public override bool Applies(StatRequest req)
	{
		if (req.HasThing && req.Thing.SpawnedOrAnyParentSpawned)
		{
			PlanetTile tile = req.Thing.MapHeld.Tile;
			if (tile.Valid)
			{
				return tile.LayerDef.isSpace;
			}
			return false;
		}
		return false;
	}
}
