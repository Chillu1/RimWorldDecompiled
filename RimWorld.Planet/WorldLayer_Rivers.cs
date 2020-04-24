using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class WorldLayer_Rivers : WorldLayer_Paths
	{
		private Color32 riverColor = new Color32(73, 82, 100, byte.MaxValue);

		private const float PerlinFrequency = 0.6f;

		private const float PerlinMagnitude = 0.1f;

		private ModuleBase riverDisplacementX = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 84905524, QualityMode.Medium);

		private ModuleBase riverDisplacementY = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 37971116, QualityMode.Medium);

		private ModuleBase riverDisplacementZ = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 91572032, QualityMode.Medium);

		public WorldLayer_Rivers()
		{
			pointyEnds = true;
		}

		public override IEnumerable Regenerate()
		{
			foreach (object item2 in base.Regenerate())
			{
				yield return item2;
			}
			LayerSubMesh subMesh = GetSubMesh(WorldMaterials.Rivers);
			LayerSubMesh subMeshBorder = GetSubMesh(WorldMaterials.RiversBorder);
			WorldGrid grid = Find.WorldGrid;
			List<OutputDirection> outputs = new List<OutputDirection>();
			List<OutputDirection> outputsBorder = new List<OutputDirection>();
			int i = 0;
			while (i < grid.TilesCount)
			{
				if (i % 1000 == 0)
				{
					yield return null;
				}
				if (subMesh.verts.Count > 60000)
				{
					subMesh = GetSubMesh(WorldMaterials.Rivers);
					subMeshBorder = GetSubMesh(WorldMaterials.RiversBorder);
				}
				Tile tile = grid[i];
				if (tile.potentialRivers != null)
				{
					outputs.Clear();
					outputsBorder.Clear();
					for (int j = 0; j < tile.potentialRivers.Count; j++)
					{
						OutputDirection item = new OutputDirection
						{
							neighbor = tile.potentialRivers[j].neighbor,
							width = tile.potentialRivers[j].river.widthOnWorld - 0.2f
						};
						outputs.Add(item);
						item = new OutputDirection
						{
							neighbor = tile.potentialRivers[j].neighbor,
							width = tile.potentialRivers[j].river.widthOnWorld
						};
						outputsBorder.Add(item);
					}
					GeneratePaths(subMesh, i, outputs, riverColor, allowSmoothTransition: true);
					GeneratePaths(subMeshBorder, i, outputsBorder, riverColor, allowSmoothTransition: true);
				}
				int num = i + 1;
				i = num;
			}
			FinalizeMesh(MeshParts.All);
		}

		public override Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity)
		{
			float magnitude = inp.magnitude;
			inp = (inp + new Vector3(riverDisplacementX.GetValue(inp), riverDisplacementY.GetValue(inp), riverDisplacementZ.GetValue(inp)) * 0.1f).normalized * magnitude;
			return inp + inp.normalized * 0.008f;
		}
	}
}
