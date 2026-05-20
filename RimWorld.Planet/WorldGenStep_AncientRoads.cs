using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_AncientRoads : WorldGenStep
{
	public float maximumSiteCurve;

	public float maximumSegmentCurviness;

	public override int SeedPart => 773428712;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		GenerateAncientRoads(layer);
	}

	private void GenerateAncientRoads(PlanetLayer layer)
	{
		Find.WorldPathGrid.RecalculateLayerPerceivedPathCosts(layer, 0);
		List<List<PlanetTile>> list = GenerateProspectiveRoads(layer);
		list.Sort((List<PlanetTile> lhs, List<PlanetTile> rhs) => -lhs.Count.CompareTo(rhs.Count));
		HashSet<PlanetTile> used = new HashSet<PlanetTile>();
		for (int num = 0; num < list.Count; num++)
		{
			if (list[num].Any((PlanetTile elem) => used.Contains(elem)))
			{
				continue;
			}
			if (list[num].Count < 4)
			{
				break;
			}
			foreach (PlanetTile item in list[num])
			{
				used.Add(item);
			}
			for (int num2 = 0; num2 < list[num].Count - 1; num2++)
			{
				float num3 = Find.WorldGrid.ApproxDistanceInTiles(list[num][num2], list[num][num2 + 1]) * maximumSegmentCurviness;
				float costCutoff = num3 * 12000f;
				using WorldPath worldPath = layer.Pather.FindPath(list[num][num2], list[num][num2 + 1], null, (float cost) => cost > costCutoff);
				if (worldPath == null || worldPath == WorldPath.NotFound)
				{
					continue;
				}
				List<PlanetTile> nodesReversed = worldPath.NodesReversed;
				if ((float)nodesReversed.Count > Find.WorldGrid.ApproxDistanceInTiles(list[num][num2], list[num][num2 + 1]) * maximumSegmentCurviness)
				{
					continue;
				}
				for (int num4 = 0; num4 < nodesReversed.Count - 1; num4++)
				{
					if (Find.WorldGrid.GetRoadDef(nodesReversed[num4], nodesReversed[num4 + 1], visibleOnly: false) != null)
					{
						Find.WorldGrid.OverlayRoad(nodesReversed[num4], nodesReversed[num4 + 1], RoadDefOf.AncientAsphaltHighway);
					}
					else
					{
						Find.WorldGrid.OverlayRoad(nodesReversed[num4], nodesReversed[num4 + 1], RoadDefOf.AncientAsphaltRoad);
					}
				}
			}
		}
	}

	private List<List<PlanetTile>> GenerateProspectiveRoads(PlanetLayer layer)
	{
		List<PlanetTile> list = Find.World.genData.ancientSites[layer];
		List<List<PlanetTile>> list2 = new List<List<PlanetTile>>();
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list.Count; j++)
			{
				List<PlanetTile> list3 = new List<PlanetTile>();
				list3.Add(list[i]);
				List<PlanetTile> list4 = list;
				float ang = Find.World.grid.GetHeadingFromTo(list[i], list[j]);
				PlanetTile current = list[i];
				while (true)
				{
					list4 = list4.Where((PlanetTile idx) => idx != current && Math.Abs(Find.World.grid.GetHeadingFromTo(current, idx) - ang) < maximumSiteCurve).ToList();
					if (list4.Count == 0)
					{
						break;
					}
					PlanetTile planetTile = list4.MinBy((PlanetTile idx) => Find.World.grid.ApproxDistanceInTiles(current, idx));
					ang = Find.World.grid.GetHeadingFromTo(current, planetTile);
					current = planetTile;
					list3.Add(current);
				}
				list2.Add(list3);
			}
		}
		return list2;
	}
}
