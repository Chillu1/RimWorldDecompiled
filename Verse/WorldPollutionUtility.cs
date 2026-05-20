using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class WorldPollutionUtility
{
	public const int NearbyPollutionTileRadius = 4;

	public static readonly SimpleCurve NearbyPollutionOverDistanceCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(2f, 1f),
		new CurvePoint(3f, 0.5f),
		new CurvePoint(4f, 0.5f)
	};

	private static readonly List<PlanetTile> TmpTileNeighbors = new List<PlanetTile>();

	private static readonly List<PlanetTile> TmpPossiblePollutableTiles = new List<PlanetTile>();

	public static void PolluteWorldAtTile(PlanetTile root, float pollutionAmount)
	{
		if (!root.Valid)
		{
			return;
		}
		PlanetTile planetTile = FindBestTileToPollute(root);
		if (planetTile.Valid)
		{
			Tile tile = Find.WorldGrid[planetTile];
			float num = tile.pollution + pollutionAmount;
			float num2 = num - 1f;
			tile.pollution = Mathf.Clamp01(num);
			MapParent mapParent = Find.WorldObjects.MapParentAt(planetTile);
			if ((mapParent == null || !mapParent.HasMap) && tile.Layer.IsRootSurface)
			{
				Vector2 vector = Find.WorldGrid.LongLatOf(planetTile);
				string text = vector.y.ToStringLatitude() + " / " + vector.x.ToStringLongitude();
				Messages.Message("MessageWorldTilePollutionChanged".Translate(pollutionAmount.ToStringPercent(), text), new LookTargets(planetTile), MessageTypeDefOf.NegativeEvent, historical: false);
			}
			Map map = Current.Game.FindMap(planetTile);
			if (map != null)
			{
				PollutionUtility.PolluteMapToPercent(map, tile.pollution);
			}
			Find.World.renderer.Notify_TilePollutionChanged(planetTile);
			if (num2 > 0f)
			{
				PolluteWorldAtTile(planetTile, num2);
			}
		}
	}

	public static PlanetTile FindBestTileToPollute(PlanetTile root)
	{
		if (!root.Valid)
		{
			return PlanetTile.Invalid;
		}
		World world = Find.World;
		WorldGrid grid = world.grid;
		if (CanPollute(root))
		{
			return root;
		}
		TmpPossiblePollutableTiles.Clear();
		int bestDistance = int.MaxValue;
		root.Layer.Filler.FloodFill(root, (PlanetTile x) => !CanPollute(x), delegate(PlanetTile t, int d)
		{
			TmpTileNeighbors.Clear();
			grid.GetTileNeighbors(t, TmpTileNeighbors);
			for (int i = 0; i < TmpTileNeighbors.Count; i++)
			{
				if (CanPollute(TmpTileNeighbors[i]) && !TmpPossiblePollutableTiles.Contains(TmpTileNeighbors[i]))
				{
					int num = Mathf.RoundToInt(grid.ApproxDistanceInTiles(root, TmpTileNeighbors[i]));
					if (num <= bestDistance)
					{
						bestDistance = num;
						TmpPossiblePollutableTiles.Add(TmpTileNeighbors[i]);
						TmpPossiblePollutableTiles.RemoveAll((PlanetTile u) => Mathf.RoundToInt(grid.ApproxDistanceInTiles(root, u)) > bestDistance);
					}
				}
			}
			return false;
		});
		PlanetTile found = (from t in TmpPossiblePollutableTiles
			orderby grid[t].PollutionLevel(), grid[t].pollution descending
			select t).FirstOrFallback(PlanetTile.Invalid);
		TmpPossiblePollutableTiles.RemoveAll((PlanetTile t) => grid[t].PollutionLevel() > grid[found].PollutionLevel() && grid[t].pollution < grid[found].pollution);
		found = TmpPossiblePollutableTiles.RandomElement();
		TmpPossiblePollutableTiles.Clear();
		TmpTileNeighbors.Clear();
		return found;
		bool CanPollute(PlanetTile t)
		{
			if (grid[t].PrimaryBiome.allowPollution)
			{
				return grid[t].pollution < 1f;
			}
			return false;
		}
	}

	public static float CalculateNearbyPollutionScore(PlanetTile tileId)
	{
		int maxTilesToProcess = Find.WorldGrid.TilesNumWithinTraversalDistance(4);
		float nearbyPollutionScore = 0f;
		tileId.Layer.Filler.FloodFill(tileId, (PlanetTile x) => true, delegate(PlanetTile tile, int dist)
		{
			nearbyPollutionScore += NearbyPollutionOverDistanceCurve.Evaluate(Mathf.RoundToInt(dist)) * Find.WorldGrid[tile].pollution;
			return false;
		}, maxTilesToProcess);
		return nearbyPollutionScore;
	}
}
