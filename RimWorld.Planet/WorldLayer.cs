using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldLayer
	{
		protected List<LayerSubMesh> subMeshes = new List<LayerSubMesh>();

		private bool dirty = true;

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		private const int MaxVerticesPerMesh = 40000;

		public virtual bool ShouldRegenerate => dirty;

		protected virtual int Layer => WorldCameraManager.WorldLayer;

		protected virtual Quaternion Rotation => Quaternion.identity;

		protected virtual float Alpha => 1f;

		public bool Dirty => dirty;

		protected LayerSubMesh GetSubMesh(Material material)
		{
			int subMeshIndex;
			return GetSubMesh(material, out subMeshIndex);
		}

		protected LayerSubMesh GetSubMesh(Material material, out int subMeshIndex)
		{
			for (int i = 0; i < subMeshes.Count; i++)
			{
				LayerSubMesh layerSubMesh = subMeshes[i];
				if (layerSubMesh.material == material && layerSubMesh.verts.Count < 40000)
				{
					subMeshIndex = i;
					return layerSubMesh;
				}
			}
			Mesh mesh = new Mesh();
			if (UnityData.isEditor)
			{
				mesh.name = "WorldLayerSubMesh_" + GetType().Name + "_" + Find.World.info.seedString;
			}
			LayerSubMesh layerSubMesh2 = new LayerSubMesh(mesh, material);
			subMeshIndex = subMeshes.Count;
			subMeshes.Add(layerSubMesh2);
			return layerSubMesh2;
		}

		protected void FinalizeMesh(MeshParts tags)
		{
			for (int i = 0; i < subMeshes.Count; i++)
			{
				if (subMeshes[i].verts.Count > 0)
				{
					subMeshes[i].FinalizeMesh(tags);
				}
			}
		}

		public void RegenerateNow()
		{
			dirty = false;
			Regenerate().ExecuteEnumerable();
		}

		public void Render()
		{
			if (ShouldRegenerate)
			{
				RegenerateNow();
			}
			int layer = Layer;
			Quaternion rotation = Rotation;
			float alpha = Alpha;
			for (int i = 0; i < subMeshes.Count; i++)
			{
				if (subMeshes[i].finalized)
				{
					if (alpha != 1f)
					{
						Color color = subMeshes[i].material.color;
						propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * alpha));
						Graphics.DrawMesh(subMeshes[i].mesh, Vector3.zero, rotation, subMeshes[i].material, layer, null, 0, propertyBlock);
					}
					else
					{
						Graphics.DrawMesh(subMeshes[i].mesh, Vector3.zero, rotation, subMeshes[i].material, layer);
					}
				}
			}
		}

		public virtual IEnumerable Regenerate()
		{
			dirty = false;
			ClearSubMeshes(MeshParts.All);
			yield break;
		}

		public void SetDirty()
		{
			dirty = true;
		}

		private void ClearSubMeshes(MeshParts parts)
		{
			for (int i = 0; i < subMeshes.Count; i++)
			{
				subMeshes[i].Clear(parts);
			}
		}
	}
}
