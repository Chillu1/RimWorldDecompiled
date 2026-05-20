using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_RiverConfluence : TileMutatorWorker_River
{
	public TileMutatorWorker_RiverConfluence(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return def.label;
	}

	protected override void GenerateRiverGraph(Map map)
	{
		if (!ModsConfig.OdysseyActive || map.TileInfo.Isnt<SurfaceTile>(out var casted))
		{
			return;
		}
		List<SurfaceTile.RiverLink> list = casted.Rivers.ToList();
		SurfaceTile.RiverLink item = list.OrderBy((SurfaceTile.RiverLink rl) => ((SurfaceTile)rl.neighbor.Tile).riverDist).First();
		list.Remove(item);
		SurfaceTile.RiverLink item2 = list.Where((SurfaceTile.RiverLink rl) => !rl.neighbor.Tile.WaterCovered).MaxBy((SurfaceTile.RiverLink rl) => rl.river.widthOnMap);
		list.Remove(item2);
		List<SurfaceTile.RiverLink> list2 = list.Where((SurfaceTile.RiverLink rl) => !rl.neighbor.Tile.WaterCovered).ToList();
		List<SurfaceTile.RiverLink> list3 = list.Where((SurfaceTile.RiverLink rl) => rl.neighbor.Tile.WaterCovered).ToList();
		float angle = Find.WorldGrid.GetHeadingFromTo(item2.neighbor.Tile.tile, item.neighbor.Tile.tile);
		Rot4 rot = Find.World.CoastDirectionAt(map.Tile);
		if (rot != Rot4.Invalid)
		{
			angle = rot.AsAngle;
		}
		(Vector3, Vector3) mapEdgeNodes = GetMapEdgeNodes(map, angle);
		Vector3 item3 = mapEdgeNodes.Item1;
		Vector3 item4 = mapEdgeNodes.Item2;
		RiverNode riverNode = ((!IsFlowingAToB(item3, item4, angle)) ? new RiverNode
		{
			start = item4,
			end = item3,
			width = item2.river.widthOnMap
		} : new RiverNode
		{
			start = item3,
			end = item4,
			width = item2.river.widthOnMap
		});
		map.waterInfo.riverGraph.Add(riverNode);
		float tValue = GetTValue(riverNode, new Vector2(riverCenter.x, riverCenter.z));
		Vector3 vector = GetDisplacedPoint(riverNode, tValue).ToVector3();
		foreach (SurfaceTile.RiverLink item9 in list2)
		{
			float headingFromTo = Find.WorldGrid.GetHeadingFromTo(item9.neighbor.Tile.tile, casted.tile);
			var (vector2, vector3) = GetMapEdgeNodes(map, headingFromTo);
			if (IsFlowingAToB(vector2, vector3, headingFromTo))
			{
				RiverNode item5 = new RiverNode
				{
					start = vector2,
					end = vector,
					width = item9.river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item5);
			}
			else
			{
				RiverNode item6 = new RiverNode
				{
					start = vector3,
					end = vector,
					width = item9.river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item6);
			}
		}
		foreach (SurfaceTile.RiverLink item10 in list3)
		{
			float headingFromTo2 = Find.WorldGrid.GetHeadingFromTo(casted.tile, item10.neighbor.Tile.tile);
			var (vector4, vector5) = GetMapEdgeNodes(map, headingFromTo2);
			if (IsFlowingAToB(vector4, vector5, headingFromTo2))
			{
				RiverNode item7 = new RiverNode
				{
					start = vector,
					end = vector5,
					width = item10.river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item7);
			}
			else
			{
				RiverNode item8 = new RiverNode
				{
					start = vector,
					end = vector4,
					width = item10.river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item8);
			}
		}
	}
}
