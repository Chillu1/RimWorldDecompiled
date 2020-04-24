using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class WorldLayer_Roads : WorldLayer_Paths
	{
		private ModuleBase roadDisplacementX = new Perlin(1.0, 2.0, 0.5, 3, 74173887, QualityMode.Medium);

		private ModuleBase roadDisplacementY = new Perlin(1.0, 2.0, 0.5, 3, 67515931, QualityMode.Medium);

		private ModuleBase roadDisplacementZ = new Perlin(1.0, 2.0, 0.5, 3, 87116801, QualityMode.Medium);

		public override IEnumerable Regenerate()
		{
			foreach (object item in base.Regenerate())
			{
				yield return item;
			}
			LayerSubMesh subMesh = GetSubMesh(WorldMaterials.Roads);
			WorldGrid grid = Find.WorldGrid;
			List<RoadWorldLayerDef> roadLayerDefs = DefDatabase<RoadWorldLayerDef>.AllDefs.OrderBy((RoadWorldLayerDef rwld) => rwld.order).ToList();
			int i = 0;
			while (i < grid.TilesCount)
			{
				if (i % 1000 == 0)
				{
					yield return null;
				}
				if (subMesh.verts.Count > 60000)
				{
					subMesh = GetSubMesh(WorldMaterials.Roads);
				}
				Tile tile = grid[i];
				if (!tile.WaterCovered)
				{
					List<OutputDirection> list = new List<OutputDirection>();
					if (tile.potentialRoads != null)
					{
						bool allowSmoothTransition = true;
						for (int j = 0; j < tile.potentialRoads.Count - 1; j++)
						{
							if (tile.potentialRoads[j].road.worldTransitionGroup != tile.potentialRoads[j + 1].road.worldTransitionGroup)
							{
								allowSmoothTransition = false;
							}
						}
						for (int k = 0; k < roadLayerDefs.Count; k++)
						{
							bool flag = false;
							list.Clear();
							for (int l = 0; l < tile.potentialRoads.Count; l++)
							{
								RoadDef road = tile.potentialRoads[l].road;
								float layerWidth = road.GetLayerWidth(roadLayerDefs[k]);
								if (layerWidth > 0f)
								{
									flag = true;
								}
								list.Add(new OutputDirection
								{
									neighbor = tile.potentialRoads[l].neighbor,
									width = layerWidth,
									distortionFrequency = road.distortionFrequency,
									distortionIntensity = road.distortionIntensity
								});
							}
							if (flag)
							{
								GeneratePaths(subMesh, i, list, roadLayerDefs[k].color, allowSmoothTransition);
							}
						}
					}
				}
				int num = i + 1;
				i = num;
			}
			FinalizeMesh(MeshParts.All);
		}

		public override Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity)
		{
			Vector3 coordinate = inp * distortionFrequency;
			float magnitude = inp.magnitude;
			Vector3 a = new Vector3(roadDisplacementX.GetValue(coordinate), roadDisplacementY.GetValue(coordinate), roadDisplacementZ.GetValue(coordinate));
			if ((double)a.magnitude > 0.0001)
			{
				float d = (1f / (1f + Mathf.Exp((0f - a.magnitude) / 1f * 2f)) * 2f - 1f) * 1f;
				a = a.normalized * d;
			}
			inp = (inp + a * distortionIntensity).normalized * magnitude;
			return inp + inp.normalized * 0.012f;
		}
	}
}
