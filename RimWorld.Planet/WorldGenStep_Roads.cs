using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Roads : WorldGenStep
{
	private struct Link
	{
		public float distance;

		public int indexA;

		public int indexB;
	}

	private class Connectedness
	{
		public Connectedness parent;

		public Connectedness Group()
		{
			if (parent == null)
			{
				return this;
			}
			return parent.Group();
		}
	}

	private static readonly FloatRange ExtraRoadNodesPer100kTiles = new FloatRange(30f, 50f);

	private static readonly IntRange RoadDistanceFromSettlement = new IntRange(-4, 4);

	private const float ChanceExtraNonSpanningTreeLink = 0.015f;

	private const float ChanceHideSpanningTreeLink = 0.1f;

	private const float ChanceWorldObjectReclusive = 0.05f;

	private const int PotentialSpanningTreeLinksPerSettlement = 8;

	public override int SeedPart => 1538475135;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		GenerateRoadEndpoints(layer);
		Rand.PushState();
		Rand.Seed = GenText.StableStringHash(seed);
		GenerateRoadNetwork(layer);
		Rand.PopState();
	}

	public override void GenerateWithoutWorldData(string seed, PlanetLayer layer)
	{
		Rand.PushState();
		Rand.Seed = GenText.StableStringHash(seed);
		GenerateRoadNetwork(layer);
		Rand.PopState();
	}

	private void GenerateRoadEndpoints(PlanetLayer layer)
	{
		List<PlanetTile> list = (from wo in Find.WorldObjects.AllWorldObjects
			where Rand.Value > 0.05f && wo.Tile.Layer == layer
			select wo.Tile).ToList();
		int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * ExtraRoadNodesPer100kTiles.RandomInRange);
		for (int num2 = 0; num2 < num; num2++)
		{
			list.Add(TileFinder.RandomSettlementTileFor(layer, null));
		}
		List<PlanetTile> list2 = new List<PlanetTile>();
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			int num4 = Mathf.Max(0, RoadDistanceFromSettlement.RandomInRange);
			PlanetTile planetTile = list[num3];
			for (int num5 = 0; num5 < num4; num5++)
			{
				Find.WorldGrid.GetTileNeighbors(planetTile, list2);
				planetTile = list2.RandomElement();
			}
			if (Find.WorldReachability.CanReach(list[num3], planetTile))
			{
				list[num3] = planetTile;
			}
		}
		list = list.Distinct().ToList();
		Find.World.genData.roadNodes[layer] = list;
	}

	private void GenerateRoadNetwork(PlanetLayer layer)
	{
		Find.WorldPathGrid.RecalculateLayerPerceivedPathCosts(layer, 0);
		List<Link> linkProspective = GenerateProspectiveLinks(Find.World.genData.roadNodes[layer], layer);
		List<Link> linkFinal = GenerateFinalLinks(linkProspective, Find.World.genData.roadNodes[layer].Count);
		DrawLinksOnWorld(layer, linkFinal, Find.World.genData.roadNodes[layer]);
	}

	private List<Link> GenerateProspectiveLinks(List<PlanetTile> indexToTile, PlanetLayer layer)
	{
		Dictionary<PlanetTile, PlanetTile> tileToIndexLookup = new Dictionary<PlanetTile, PlanetTile>();
		for (int i = 0; i < indexToTile.Count; i++)
		{
			tileToIndexLookup[indexToTile[i]] = new PlanetTile(i, layer);
		}
		List<Link> linkProspective = new List<Link>();
		List<PlanetTile> list = new List<PlanetTile>();
		for (int j = 0; j < indexToTile.Count; j++)
		{
			int srcLocal = j;
			PlanetTile srcTile = indexToTile[j];
			list.Clear();
			list.Add(srcTile);
			int found = 0;
			layer.Pather.FloodPathsWithCost(list, (PlanetTile src, PlanetTile dst) => Caravan_PathFollower.CostToMove(3300, src, dst, null, perceivedStatic: true), null, delegate(PlanetTile tile, float distance)
			{
				if (tile != srcTile && tileToIndexLookup.TryGetValue(tile, out var value))
				{
					int num = found + 1;
					found = num;
					linkProspective.Add(new Link
					{
						distance = distance,
						indexA = srcLocal,
						indexB = value.tileId
					});
				}
				return found >= 8;
			});
		}
		linkProspective.Sort((Link lhs, Link rhs) => lhs.distance.CompareTo(rhs.distance));
		return linkProspective;
	}

	private List<Link> GenerateFinalLinks(List<Link> linkProspective, int endpointCount)
	{
		List<Connectedness> list = new List<Connectedness>();
		for (int i = 0; i < endpointCount; i++)
		{
			list.Add(new Connectedness());
		}
		List<Link> list2 = new List<Link>();
		for (int j = 0; j < linkProspective.Count; j++)
		{
			Link prospective = linkProspective[j];
			if (list[prospective.indexA].Group() != list[prospective.indexB].Group() || (!(Rand.Value > 0.015f) && !list2.Any((Link link) => link.indexB == prospective.indexA && link.indexA == prospective.indexB)))
			{
				if (Rand.Value > 0.1f)
				{
					list2.Add(prospective);
				}
				if (list[prospective.indexA].Group() != list[prospective.indexB].Group())
				{
					Connectedness parent = new Connectedness();
					list[prospective.indexA].Group().parent = parent;
					list[prospective.indexB].Group().parent = parent;
				}
			}
		}
		return list2;
	}

	private void DrawLinksOnWorld(PlanetLayer layer, List<Link> linkFinal, List<PlanetTile> indexToTile)
	{
		foreach (Link item in linkFinal)
		{
			WorldPath worldPath = layer.Pather.FindPath(indexToTile[item.indexA], indexToTile[item.indexB], null);
			List<PlanetTile> nodesReversed = worldPath.NodesReversed;
			RoadDef roadDef = DefDatabase<RoadDef>.AllDefsListForReading.Where((RoadDef rd) => !rd.ancientOnly).RandomElementWithFallback();
			for (int num = 0; num < nodesReversed.Count - 1; num++)
			{
				Find.WorldGrid.OverlayRoad(nodesReversed[num], nodesReversed[num + 1], roadDef);
			}
			worldPath.ReleaseToPool();
		}
	}
}
