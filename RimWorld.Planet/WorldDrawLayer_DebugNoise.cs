using System.Collections;
using System.Collections.Generic;
using LudeonTK;
using Unity.Collections;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet;

public class WorldDrawLayer_DebugNoise : WorldDrawLayer
{
	private readonly List<List<int>> triangleIndexToTileID = new List<List<int>>();

	private Material material;

	public override bool Visible
	{
		get
		{
			if (base.Visible && TryGetWindow(out var window))
			{
				return window.alpha > 0f;
			}
			return false;
		}
	}

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;

	private bool TryGetWindow(out Dialog_DevNoiseWorld window)
	{
		return Find.WindowStack.TryGetWindow<Dialog_DevNoiseWorld>(out window);
	}

	public override IEnumerable Regenerate()
	{
		if (material == null)
		{
			material = new Material(WorldMaterials.VertexColorTransparent);
		}
		TryGetWindow(out var window);
		Perlin perlin = window.noise;
		if (perlin == null)
		{
			perlin = new Perlin(0.017000000923871994, 2.0, 0.5, 6, normalized: true, invert: false, Find.World.info.Seed, QualityMode.Medium);
		}
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		NativeArray<int> unsafeTileIDToVerts_offsets = planetLayer.UnsafeTileIDToVerts_offsets;
		NativeArray<Vector3> unsafeVerts = planetLayer.UnsafeVerts;
		byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(window.alpha * 255.999f), 0, 255);
		triangleIndexToTileID.Clear();
		for (int i = 0; i < planetLayer.TilesCount; i++)
		{
			Vector3 tileCenter = Find.WorldGrid.GetTileCenter(new PlanetTile(i, planetLayer));
			float num = (float)perlin.GetValue(tileCenter.x, tileCenter.y, tileCenter.z);
			byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(num * 255.999f), 0, 255);
			if (window.cutoff != 0f && num < window.cutoff && window.cutoffEnabled)
			{
				continue;
			}
			int subMeshIndex;
			LayerSubMesh subMesh = GetSubMesh(material, out subMeshIndex);
			while (subMeshIndex >= triangleIndexToTileID.Count)
			{
				triangleIndexToTileID.Add(new List<int>());
			}
			int count = subMesh.verts.Count;
			int num2 = 0;
			int num3 = ((i + 1 < unsafeTileIDToVerts_offsets.Length) ? unsafeTileIDToVerts_offsets[i + 1] : unsafeVerts.Length);
			for (int j = unsafeTileIDToVerts_offsets[i]; j < num3; j++)
			{
				subMesh.verts.Add(unsafeVerts[j]);
				subMesh.colors.Add(new Color32(b, b, b, a));
				if (j < num3 - 2)
				{
					subMesh.tris.Add(count + num2 + 2);
					subMesh.tris.Add(count + num2 + 1);
					subMesh.tris.Add(count);
					triangleIndexToTileID[subMeshIndex].Add(i);
				}
				num2++;
			}
		}
		FinalizeMesh(MeshParts.All);
	}
}
