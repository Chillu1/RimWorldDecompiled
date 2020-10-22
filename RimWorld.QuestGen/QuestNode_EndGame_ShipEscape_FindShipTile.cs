using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_EndGame_ShipEscape_FindShipTile : QuestNode
	{
		private const int MinTraversalDistance = 180;

		private const int MaxTraversalDistance = 800;

		[NoTranslate]
		public SlateRef<string> storeAs;

		private bool TryFindRootTile(out int tile)
		{
			int tile2;
			return TileFinder.TryFindRandomPlayerTile(out tile, allowCaravans: false, (int x) => TryFindDestinationTileActual(x, 180, out tile2));
		}

		private bool TryFindDestinationTile(int rootTile, out int tile)
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
			tile = -1;
			return false;
		}

		private bool TryFindDestinationTileActual(int rootTile, int minDist, out int tile)
		{
			for (int i = 0; i < 2; i++)
			{
				bool canTraverseImpassable = i == 1;
				if (TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, 800, out tile, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && Find.WorldGrid[x].biome.canBuildBase && Find.WorldGrid[x].biome.canAutoChoose, ignoreFirstTilePassability: true, preferCloserTiles: true, canTraverseImpassable))
				{
					return true;
				}
			}
			tile = -1;
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
			int tile2;
			if (TryFindRootTile(out var tile))
			{
				return TryFindDestinationTile(tile, out tile2);
			}
			return false;
		}
	}
}
