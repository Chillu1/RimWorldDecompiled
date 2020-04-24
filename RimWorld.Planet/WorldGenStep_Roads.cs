using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
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

		public override void GenerateFresh(string seed)
		{
			GenerateRoadEndpoints();
			Rand.PushState();
			Rand.Seed = GenText.StableStringHash(seed);
			GenerateRoadNetwork();
			Rand.PopState();
		}

		public override void GenerateWithoutWorldData(string seed)
		{
			Rand.PushState();
			Rand.Seed = GenText.StableStringHash(seed);
			GenerateRoadNetwork();
			Rand.PopState();
		}

		private void GenerateRoadEndpoints()
		{
			List<int> list = (from wo in Find.WorldObjects.AllWorldObjects
				where Rand.Value > 0.05f
				select wo.Tile).ToList();
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * ExtraRoadNodesPer100kTiles.RandomInRange);
			for (int i = 0; i < num; i++)
			{
				list.Add(TileFinder.RandomSettlementTileFor(null));
			}
			List<int> list2 = new List<int>();
			for (int j = 0; j < list.Count; j++)
			{
				int num2 = Mathf.Max(0, RoadDistanceFromSettlement.RandomInRange);
				int num3 = list[j];
				for (int k = 0; k < num2; k++)
				{
					Find.WorldGrid.GetTileNeighbors(num3, list2);
					num3 = list2.RandomElement();
				}
				if (Find.WorldReachability.CanReach(list[j], num3))
				{
					list[j] = num3;
				}
			}
			list = list.Distinct().ToList();
			Find.World.genData.roadNodes = list;
		}

		private void GenerateRoadNetwork()
		{
			Find.WorldPathGrid.RecalculateAllPerceivedPathCosts(0);
			List<Link> linkProspective = GenerateProspectiveLinks(Find.World.genData.roadNodes);
			List<Link> linkFinal = GenerateFinalLinks(linkProspective, Find.World.genData.roadNodes.Count);
			DrawLinksOnWorld(linkFinal, Find.World.genData.roadNodes);
		}

		private List<Link> GenerateProspectiveLinks(List<int> indexToTile)
		{
			Dictionary<int, int> tileToIndexLookup = new Dictionary<int, int>();
			for (int i = 0; i < indexToTile.Count; i++)
			{
				tileToIndexLookup[indexToTile[i]] = i;
			}
			List<Link> linkProspective = new List<Link>();
			List<int> list = new List<int>();
			int srcIndex = 0;
			while (srcIndex < indexToTile.Count)
			{
				int srcTile = indexToTile[srcIndex];
				list.Clear();
				list.Add(srcTile);
				int found = 0;
				Find.WorldPathFinder.FloodPathsWithCost(list, (int src, int dst) => Caravan_PathFollower.CostToMove(3300, src, dst, null, perceivedStatic: true), null, delegate(int tile, float distance)
				{
					if (tile != srcTile && tileToIndexLookup.ContainsKey(tile))
					{
						int num2 = ++found;
						linkProspective.Add(new Link
						{
							distance = distance,
							indexA = srcIndex,
							indexB = tileToIndexLookup[tile]
						});
					}
					return found >= 8;
				});
				int num = ++srcIndex;
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

		private void DrawLinksOnWorld(List<Link> linkFinal, List<int> indexToTile)
		{
			foreach (Link item in linkFinal)
			{
				WorldPath worldPath = Find.WorldPathFinder.FindPath(indexToTile[item.indexA], indexToTile[item.indexB], null);
				List<int> nodesReversed = worldPath.NodesReversed;
				RoadDef roadDef = DefDatabase<RoadDef>.AllDefsListForReading.Where((RoadDef rd) => !rd.ancientOnly).RandomElementWithFallback();
				for (int i = 0; i < nodesReversed.Count - 1; i++)
				{
					Find.WorldGrid.OverlayRoad(nodesReversed[i], nodesReversed[i + 1], roadDef);
				}
				worldPath.ReleaseToPool();
			}
		}
	}
}
