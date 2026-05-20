using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet;

public class WorldDrawLayer_Rivers : WorldDrawLayer_Paths
{
	private readonly Color32 riverColor = new Color32(73, 82, 100, byte.MaxValue);

	private const float PerlinFrequency = 0.6f;

	private const float PerlinMagnitude = 0.1f;

	private readonly ModuleBase riverDisplacementX = new Perlin(0.6000000238418579, 2.0, 0.5, 3, 84905524, QualityMode.Medium);

	private readonly ModuleBase riverDisplacementY = new Perlin(0.6000000238418579, 2.0, 0.5, 3, 37971116, QualityMode.Medium);

	private readonly ModuleBase riverDisplacementZ = new Perlin(0.6000000238418579, 2.0, 0.5, 3, 91572032, QualityMode.Medium);

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;

	public WorldDrawLayer_Rivers()
	{
		pointyEnds = true;
	}

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		LayerSubMesh subMesh = GetSubMesh(WorldMaterials.Rivers);
		LayerSubMesh subMeshBorder = GetSubMesh(WorldMaterials.RiversBorder);
		List<OutputDirection> outputs = new List<OutputDirection>();
		List<OutputDirection> outputsBorder = new List<OutputDirection>();
		int i = 0;
		while (i < planetLayer.TilesCount)
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
			SurfaceTile surfaceTile = (SurfaceTile)planetLayer[i];
			if (surfaceTile.potentialRivers != null)
			{
				outputs.Clear();
				outputsBorder.Clear();
				for (int j = 0; j < surfaceTile.potentialRivers.Count; j++)
				{
					outputs.Add(new OutputDirection
					{
						neighbor = surfaceTile.potentialRivers[j].neighbor,
						width = surfaceTile.potentialRivers[j].river.widthOnWorld - 0.2f
					});
					outputsBorder.Add(new OutputDirection
					{
						neighbor = surfaceTile.potentialRivers[j].neighbor,
						width = surfaceTile.potentialRivers[j].river.widthOnWorld
					});
				}
				GeneratePaths(subMesh, surfaceTile.tile, outputs, riverColor, allowSmoothTransition: true);
				GeneratePaths(subMeshBorder, surfaceTile.tile, outputsBorder, riverColor, allowSmoothTransition: true);
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
		return inp + inp.normalized * 0.01f;
	}
}
