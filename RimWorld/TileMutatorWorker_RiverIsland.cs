using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TileMutatorWorker_RiverIsland : TileMutatorWorker_River
	{
		private const float MinWidth = 1.5f;

		private const float RiverCurveAmplitude = 20f;

		private static readonly FloatRange RiverIslandAmplitudeRange = new FloatRange(1f, 2f);

		private static readonly FloatRange RiverIslandCentralityRange = new FloatRange(0f, 0.3f);

		private static readonly FloatRange RiverWidthFracRange = new FloatRange(0.2f, 0.8f);

		private RiverNode nodeA;

		private RiverNode nodeB;

		private float widthFrac;

		private float riverIslandAmplitude;

		private float riverIslandCentrality;

		protected override float GetCurveAmplitude => 20f;

		public override string GetLabel(PlanetTile tile)
		{
			return def.label;
		}

		public TileMutatorWorker_RiverIsland(TileMutatorDef def)
			: base(def)
		{
		}

		public override void Init(Map map)
		{
			base.Init(map);
			riverIslandAmplitude = RiverIslandAmplitudeRange.RandomInRange;
			riverIslandCentrality = RiverIslandCentralityRange.RandomInRange;
			widthFrac = RiverWidthFracRange.RandomInRange;
		}

		protected override void GenerateRiverGraph(Map map)
		{
			if (ModsConfig.OdysseyActive && !map.TileInfo.Isnt<SurfaceTile>(out var casted))
			{
				List<SurfaceTile.RiverLink> source = casted.Rivers.OrderBy((SurfaceTile.RiverLink rl) => -((SurfaceTile)rl.neighbor.Tile).riverDist).ToList();
				float headingFromTo = Find.WorldGrid.GetHeadingFromTo(source.First().neighbor.Tile.tile, source.Last().neighbor.Tile.tile);
				var (vector, vector2) = GetMapEdgeNodes(map, headingFromTo);
				if (IsFlowingAToB(vector, vector2, headingFromTo))
				{
					nodeA = new RiverNode
					{
						start = vector,
						end = vector2,
						width = source.First().river.widthOnMap
					};
					nodeB = new RiverNode
					{
						start = vector,
						end = vector2,
						width = source.First().river.widthOnMap
					};
				}
				else
				{
					nodeA = new RiverNode
					{
						start = vector2,
						end = vector,
						width = source.First().river.widthOnMap
					};
					nodeB = new RiverNode
					{
						start = vector2,
						end = vector,
						width = source.First().river.widthOnMap
					};
				}
				map.waterInfo.riverGraph.Add(nodeA);
				map.waterInfo.riverGraph.Add(nodeB);
			}
		}

		protected override Vector2 GetDisplacedPoint(RiverNode riverNode, float t)
		{
			Vector3 vector = riverNode.end - riverNode.start;
			Vector3 vector2 = riverNode.start + t * vector;
			float num = -4f * Mathf.Pow(t, 2f) + 4f * t;
			float num2 = t * Vector3.Distance(riverNode.start, riverNode.end);
			Vector3 normalized = new Vector3(0f - vector.z, 0f, vector.x).normalized;
			float num3 = Mathf.InverseLerp(riverIslandCentrality, 1f - riverIslandCentrality, t);
			float num4 = (float)riverBendNoise.GetValue(num2 * GetWidthNoiseFactor(riverNode), 0.0, riverNode.seed);
			float num5 = -4f * Mathf.Pow(num3, 2f) + 4f * num3;
			num4 = ((riverNode != nodeA) ? (num4 - riverIslandAmplitude * num5) : (num4 + riverIslandAmplitude * num5));
			Vector3 vector3 = vector2 + num4 * GetCurveAmplitude * normalized * num;
			return new Vector2(vector3.x, vector3.z);
		}

		protected override float GetRiverWidthAt(RiverNode riverNode, Vector2 cell)
		{
			float tValue = GetTValue(riverNode, cell);
			float num = ((riverNode == nodeA) ? widthFrac : (1f - widthFrac));
			float num2 = Mathf.InverseLerp(riverIslandCentrality, 1f - riverIslandCentrality, tValue);
			float num3 = -4f * Mathf.Pow(num2, 2f) + 4f * num2;
			float num4 = 1f - num * num3;
			return Mathf.Max(riverNode.width / 2f * num4 * (1f + riverWidthNoise.GetValue(cell) * 0.15f), 1.5f);
		}
	}
}
