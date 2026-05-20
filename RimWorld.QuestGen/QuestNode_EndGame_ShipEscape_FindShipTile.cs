using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_EndGame_ShipEscape_FindShipTile : QuestNode
{
	private const int MinTraversalDistance = 180;

	private const int MaxTraversalDistance = 800;

	[NoTranslate]
	public SlateRef<string> storeAs;

	private bool TryFindRootTile(out PlanetTile tile)
	{
		PlanetTile tile2;
		return TileFinder.TryFindRandomPlayerTile(out tile, allowCaravans: false, (PlanetTile x) => TryFindDestinationTileActual(x, 180, out tile2));
	}

	private bool TryFindDestinationTile(PlanetTile rootTile, out PlanetTile tile)
	{
		int num = 800;
		for (int i = 0; i < 1000; i++)
		{
			num = (int)((float)num * Rand.Range(0.5f, 0.75f));
			if (num <= 180)
			{
				num = 180;
			}
			if (TryFindDestinationTileActual(rootTile, num, out tile))
			{
				return true;
			}
			if (num <= 180)
			{
				return false;
			}
		}
		tile = PlanetTile.Invalid;
		return false;
	}

	private bool TryFindDestinationTileActual(PlanetTile rootTile, int minDist, out PlanetTile tile)
	{
		for (int i = 0; i < 2; i++)
		{
			bool canTraverseImpassable = i == 1;
			if (TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, 800, out tile, (PlanetTile x) => !Find.WorldObjects.AnyWorldObjectAt(x) && Find.WorldGrid[x].PrimaryBiome.canBuildBase && Find.WorldGrid[x].PrimaryBiome.canAutoChoose, ignoreFirstTilePassability: true, TileFinderMode.Near, canTraverseImpassable))
			{
				return true;
			}
		}
		tile = PlanetTile.Invalid;
		return false;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		TryFindRootTile(out var tile);
		TryFindDestinationTile(tile, out var tile2);
		slate.Set(storeAs.GetValue(slate), tile2);
	}

	protected override bool TestRunInt(Slate slate)
	{
		PlanetTile tile2;
		if (TryFindRootTile(out var tile))
		{
			return TryFindDestinationTile(tile, out tile2);
		}
		return false;
	}
}
