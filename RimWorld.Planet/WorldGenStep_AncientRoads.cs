using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_AncientRoads : WorldGenStep
	{
		public float maximumSiteCurve;

		public float minimumChain;

		public float maximumSegmentCurviness;

		public override int SeedPart => 773428712;

		public override void GenerateFresh(string seed)
		{
			GenerateAncientRoads();
		}

		private void GenerateAncientRoads()
		{
			Find.WorldPathGrid.RecalculateAllPerceivedPathCosts(0);
			List<List<int>> list = GenerateProspectiveRoads();
			list.Sort((List<int> lhs, List<int> rhs) => -lhs.Count.CompareTo(rhs.Count));
			HashSet<int> used = new HashSet<int>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Any((int elem) => used.Contains(elem)))
				{
					continue;
				}
				if (list[i].Count < 4)
				{
					break;
				}
				foreach (int item in list[i])
				{
					used.Add(item);
				}
				for (int j = 0; j < list[i].Count - 1; j++)
				{
					float num = Find.WorldGrid.ApproxDistanceInTiles(list[i][j], list[i][j + 1]) * maximumSegmentCurviness;
					float costCutoff = num * 12000f;
					using WorldPath worldPath = Find.WorldPathFinder.FindPath(list[i][j], list[i][j + 1], null, (float cost) => cost > costCutoff);
					if (worldPath == null || worldPath == WorldPath.NotFound)
					{
						continue;
					}
					List<int> nodesReversed = worldPath.NodesReversed;
					if ((float)nodesReversed.Count > Find.WorldGrid.ApproxDistanceInTiles(list[i][j], list[i][j + 1]) * maximumSegmentCurviness)
					{
						continue;
					}
					for (int k = 0; k < nodesReversed.Count - 1; k++)
					{
						if (Find.WorldGrid.GetRoadDef(nodesReversed[k], nodesReversed[k + 1], visibleOnly: false) != null)
						{
							Find.WorldGrid.OverlayRoad(nodesReversed[k], nodesReversed[k + 1], RoadDefOf.AncientAsphaltHighway);
						}
						else
						{
							Find.WorldGrid.OverlayRoad(nodesReversed[k], nodesReversed[k + 1], RoadDefOf.AncientAsphaltRoad);
						}
					}
				}
			}
		}

		private List<List<int>> GenerateProspectiveRoads()
		{
			List<int> ancientSites = Find.World.genData.ancientSites;
			List<List<int>> list = new List<List<int>>();
			for (int i = 0; i < ancientSites.Count; i++)
			{
				for (int j = 0; j < ancientSites.Count; j++)
				{
					List<int> list2 = new List<int>();
					list2.Add(ancientSites[i]);
					List<int> list3 = ancientSites;
					float ang = Find.World.grid.GetHeadingFromTo(i, j);
					int current = ancientSites[i];
					while (true)
					{
						list3 = list3.Where((int idx) => idx != current && Math.Abs(Find.World.grid.GetHeadingFromTo(current, idx) - ang) < maximumSiteCurve).ToList();
						if (list3.Count == 0)
						{
							break;
						}
						int num = list3.MinBy((int idx) => Find.World.grid.ApproxDistanceInTiles(current, idx));
						ang = Find.World.grid.GetHeadingFromTo(current, num);
						current = num;
						list2.Add(current);
					}
					list.Add(list2);
				}
			}
			return list;
		}
	}
}
