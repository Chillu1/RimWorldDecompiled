using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_SpaceHabitat : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.OdysseyActive || !p.SpawnedOrAnyParentSpawned)
		{
			return ThoughtState.Inactive;
		}
		PlanetTile tile = p.Tile;
		if (tile.Valid && tile.LayerDef.isSpace)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		return ThoughtState.ActiveAtStage(0);
	}
}
