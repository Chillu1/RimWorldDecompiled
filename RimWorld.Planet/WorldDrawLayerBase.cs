using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public abstract class WorldDrawLayerBase
{
	protected bool dirty = true;

	private MaterialPropertyBlock propertyBlock;

	protected readonly List<LayerSubMesh> subMeshes = new List<LayerSubMesh>();

	private const int MaxVerticesPerMesh = 40000;

	public virtual bool ShouldRegenerate
	{
		get
		{
			if (dirty)
			{
				return Visible;
			}
			return false;
		}
	}

	protected virtual int RenderLayer => WorldCameraManager.WorldLayer;

	protected virtual Quaternion Rotation => Quaternion.identity;

	protected virtual float Alpha => 1f;

	public bool Dirty => dirty;

	public virtual bool Visible
	{
		get
		{
			if (WorldRendererUtility.WorldBackgroundNow)
			{
				return VisibleInBackground;
			}
			return true;
		}
	}

	public virtual bool VisibleWhenLayerNotSelected => true;

	public virtual bool VisibleInBackground => true;

	public virtual Vector3 Position
	{
		get
		{
			if (CameraIsOrigin)
			{
				return Find.WorldCameraDriver.CameraPosition;
			}
			return Vector3.zero;
		}
	}

	public virtual bool CameraIsOrigin => false;

	public MaterialPropertyBlock MatPropBlock => propertyBlock ?? (propertyBlock = new MaterialPropertyBlock());

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

	public virtual void Render()
	{
		if (!Visible)
		{
			return;
		}
		if (ShouldRegenerate)
		{
			RegenerateNow();
		}
		int renderLayer = RenderLayer;
		Vector3 position = Position;
		Quaternion rotation = Rotation;
		float alpha = Alpha;
		for (int i = 0; i < subMeshes.Count; i++)
		{
			if (subMeshes[i].finalized)
			{
				if (!Mathf.Approximately(alpha, 1f))
				{
					Color color = subMeshes[i].material.color;
					MatPropBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * alpha));
					Graphics.DrawMesh(subMeshes[i].mesh, position, rotation, subMeshes[i].material, renderLayer, null, 0, MatPropBlock);
				}
				else
				{
					Graphics.DrawMesh(subMeshes[i].mesh, position, rotation, subMeshes[i].material, renderLayer);
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

	public void Dispose()
	{
		foreach (LayerSubMesh subMesh in subMeshes)
		{
			Object.Destroy(subMesh.mesh);
		}
		subMeshes.Clear();
	}
}
