using System.Collections;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_RaycastableGrid : WorldDrawLayer
{
	private Material material;

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;

	public override IEnumerable Regenerate()
	{
		if (material == null)
		{
			material = new Material(WorldMaterials.VertexColorTransparent);
		}
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		NativeArray<int> unsafeTileIDToVerts_offsets = planetLayer.UnsafeTileIDToVerts_offsets;
		NativeArray<Vector3> unsafeVerts = planetLayer.UnsafeVerts;
		for (int i = 0; i < planetLayer.TilesCount; i++)
		{
			int subMeshIndex;
			LayerSubMesh subMesh = GetSubMesh(material, out subMeshIndex);
			int count = subMesh.verts.Count;
			int num = 0;
			int num2 = ((i + 1 < unsafeTileIDToVerts_offsets.Length) ? unsafeTileIDToVerts_offsets[i + 1] : unsafeVerts.Length);
			_ = unsafeTileIDToVerts_offsets[i];
			for (int j = unsafeTileIDToVerts_offsets[i]; j < num2; j++)
			{
				subMesh.verts.Add(unsafeVerts[j]);
				subMesh.colors.Add(new Color32(200, 200, 200, 10));
				switch (num)
				{
				case 0:
					subMesh.uvs.Add(new Vector3(1f, 0f, 0f));
					break;
				case 1:
					subMesh.uvs.Add(new Vector3(0f, 1f, 0f));
					break;
				default:
					subMesh.uvs.Add(new Vector3(0f, 0f, 1f));
					break;
				}
				if (j < num2 - 2)
				{
					subMesh.tris.Add(count + num + 2);
					subMesh.tris.Add(count + num + 1);
					subMesh.tris.Add(count);
					AppendRaycastableTriangle(subMeshIndex, i);
				}
				num++;
			}
		}
		FinalizeMesh(MeshParts.All);
		foreach (object item2 in RegenerateWorldMeshColliders())
		{
			yield return item2;
		}
	}

	public override void Render()
	{
		if (ShouldRegenerate)
		{
			RegenerateNow();
		}
	}
}
