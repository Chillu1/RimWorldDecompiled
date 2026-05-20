using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_RiverDelta : TileMutatorWorker_River
{
	private const float MinWidth = 1.5f;

	private const float DeltaCurveFrequency = 0.02f;

	private const float DeltaCurveAmplitude = 10f;

	private const float DeltaMergeDist = 25f;

	private const float DeltaSplitChance = 0.9f;

	private const float MinSegmentLength = 30f;

	private const float MaxSegmentLength = 70f;

	private const float MinSplitAngle = 15f;

	private const float MaxSplitAngle = 30f;

	private const float MinInitialLength = 0.2f;

	private const float MaxInitialLength = 0.5f;

	private const float ExtraWidthAngle = 5f;

	private static FloatRange SegmentLengthRange => new FloatRange(30f, 70f);

	private static FloatRange SplitAngleRange => new FloatRange(15f, 30f);

	private static FloatRange InitialLengthRange => new FloatRange(0.2f, 0.5f);

	protected override float GetCurveFrequency => 0.02f;

	protected override float GetCurveAmplitude => 10f;

	protected override float GetWidthNoiseFactor(RiverNode riverNode)
	{
		return 1f;
	}

	public TileMutatorWorker_RiverDelta(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return def.label;
	}

	protected override void GenerateRiverGraph(Map map)
	{
		if (ModsConfig.OdysseyActive && !map.TileInfo.Isnt<SurfaceTile>(out var casted))
		{
			SurfaceTile.RiverLink riverLink = casted.Rivers.Where((SurfaceTile.RiverLink rl) => !rl.neighbor.Tile.WaterCovered).MaxBy((SurfaceTile.RiverLink rl) => rl.river.widthOnMap);
			float num = ((Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean) ?? Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Lake)).Value + 180f) % 360f;
			float angle = (450f - num) % 360f;
			(Vector3, Vector3) mapEdgeNodes = GetMapEdgeNodes(map, angle);
			Vector3 item = mapEdgeNodes.Item1;
			Vector3 item2 = mapEdgeNodes.Item2;
			RiverNode riverNode = ((!IsFlowingAToB(item, item2, angle)) ? new RiverNode
			{
				start = item2,
				end = item2 + (item - item2) * InitialLengthRange.RandomInRange,
				width = riverLink.river.widthOnMap
			} : new RiverNode
			{
				start = item,
				end = item + (item2 - item) * InitialLengthRange.RandomInRange,
				width = riverLink.river.widthOnMap
			});
			map.waterInfo.riverGraph.Add(riverNode);
			SplitRiver(map, riverNode, num);
			MergeNearbyNodes(map, riverNode);
		}
	}

	private void SplitRiver(Map map, RiverNode prevSegment, float baseAngle)
	{
		if (prevSegment.end.InBounds(map) && !map.terrainGrid.TerrainAt(prevSegment.end.ToIntVec3()).IsOcean)
		{
			Vector3 normalized = (prevSegment.end - prevSegment.start).normalized;
			Vector3 vector = new Vector3(0f - normalized.z, 0f, normalized.x);
			float num = 5f * (prevSegment.width / 10f);
			if (!Rand.Chance(0.9f))
			{
				float f = (baseAngle + SplitAngleRange.RandomInRange - SplitAngleRange.Average) * (MathF.PI / 180f);
				float randomInRange = SegmentLengthRange.RandomInRange;
				float width = prevSegment.width;
				Vector3 start = prevSegment.end - normalized * width / 4f;
				Vector3 end = new Vector3(prevSegment.end.x + randomInRange * Mathf.Cos(f), 0f, prevSegment.end.z + randomInRange * Mathf.Sin(f));
				RiverNode riverNode = new RiverNode
				{
					start = start,
					end = end,
					width = width
				};
				map.waterInfo.riverGraph.Add(riverNode);
				prevSegment.childNodes.Add(riverNode);
				SplitRiver(map, riverNode, baseAngle);
				return;
			}
			float f2 = (baseAngle - SplitAngleRange.RandomInRange - num) * (MathF.PI / 180f);
			float randomInRange2 = SegmentLengthRange.RandomInRange;
			float num2 = Mathf.Max(prevSegment.width * Rand.Range(0.33f, 0.67f), 1.5f);
			Vector3 start2 = prevSegment.end - normalized * num2 / 4f - vector * (prevSegment.width / 2f - num2 / 2f);
			Vector3 end2 = new Vector3(prevSegment.end.x + randomInRange2 * Mathf.Cos(f2), 0f, prevSegment.end.z + randomInRange2 * Mathf.Sin(f2));
			RiverNode riverNode2 = new RiverNode
			{
				start = start2,
				end = end2,
				width = num2 * 1.1f
			};
			map.waterInfo.riverGraph.Add(riverNode2);
			prevSegment.childNodes.Add(riverNode2);
			SplitRiver(map, riverNode2, baseAngle);
			f2 = (baseAngle + SplitAngleRange.RandomInRange + num) * (MathF.PI / 180f);
			randomInRange2 = SegmentLengthRange.RandomInRange;
			num2 = Mathf.Max(prevSegment.width - num2, 1.5f) + 1f;
			Vector3 start3 = prevSegment.end - normalized * num2 / 4f + vector * (prevSegment.width / 2f - num2 / 2f);
			Vector3 end3 = new Vector3(prevSegment.end.x + randomInRange2 * Mathf.Cos(f2), 0f, prevSegment.end.z + randomInRange2 * Mathf.Sin(f2));
			RiverNode riverNode3 = new RiverNode
			{
				start = start3,
				end = end3,
				width = num2 * 1.1f
			};
			map.waterInfo.riverGraph.Add(riverNode3);
			prevSegment.childNodes.Add(riverNode3);
			SplitRiver(map, riverNode3, baseAngle);
		}
	}

	private void MergeNearbyNodes(Map map, RiverNode root)
	{
		HashSet<RiverNode> markedForRemoval = new HashSet<RiverNode>();
		List<Vector3> list = new List<Vector3>();
		Queue<RiverNode> queue = new Queue<RiverNode>();
		queue.Enqueue(root);
		while (!queue.Empty())
		{
			RiverNode riverNode = queue.Dequeue();
			bool flag = false;
			foreach (Vector3 item in list)
			{
				if (!((riverNode.end - item).magnitude < 25f))
				{
					continue;
				}
				riverNode.end = item;
				foreach (RiverNode childNode in riverNode.childNodes)
				{
					RecursivelyMarkForRemoval(childNode);
				}
				flag = true;
			}
			if (flag)
			{
				continue;
			}
			list.Add(riverNode.end);
			foreach (RiverNode item2 in riverNode.childNodes.InRandomOrder())
			{
				queue.Enqueue(item2);
			}
		}
		foreach (RiverNode item3 in markedForRemoval)
		{
			map.waterInfo.riverGraph.Remove(item3);
		}
		void RecursivelyMarkForRemoval(RiverNode node)
		{
			markedForRemoval.Add(node);
			foreach (RiverNode childNode2 in node.childNodes)
			{
				RecursivelyMarkForRemoval(childNode2);
			}
		}
	}
}
