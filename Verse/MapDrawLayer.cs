using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public abstract class MapDrawLayer
{
	private Map map;

	public ulong relevantChangeTypes;

	public List<LayerSubMesh> subMeshes = new List<LayerSubMesh>();

	protected Map Map => map;

	public virtual bool Visible => true;

	public bool Dirty { get; set; }

	public MapDrawLayer(Map map)
	{
		this.map = map;
	}

	public static LayerSubMesh CreateFreeSubMesh(Material material, Map map = null)
	{
		if (material == null)
		{
			return null;
		}
		Mesh mesh = new Mesh();
		if (UnityData.isEditor)
		{
			mesh.name = "SectionLayerSubMesh_Free" + ((map == null) ? "" : ("_" + map.Tile));
		}
		return new LayerSubMesh(mesh, material);
	}

	public LayerSubMesh GetSubMesh(Material material)
	{
		if (material == null)
		{
			return null;
		}
		for (int i = 0; i < subMeshes.Count; i++)
		{
			if (subMeshes[i].material == material)
			{
				return subMeshes[i];
			}
		}
		Mesh mesh = new Mesh();
		if (UnityData.isEditor)
		{
			mesh.name = "SectionLayerSubMesh_" + GetType().Name + "_" + Map.Tile;
		}
		LayerSubMesh layerSubMesh = new LayerSubMesh(mesh, material);
		subMeshes.Add(layerSubMesh);
		return layerSubMesh;
	}

	public virtual CellRect GetBoundaryRect()
	{
		return map.BoundsRect();
	}

	public virtual void RefreshSubMeshBounds()
	{
		Bounds bounds = GetBoundaryRect().ExpandedBy(2).ToBounds();
		for (int i = 0; i < subMeshes.Count; i++)
		{
			subMeshes[i].mesh.bounds = bounds;
		}
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

	public virtual void DrawLayer()
	{
		if (!Visible)
		{
			return;
		}
		int count = subMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			LayerSubMesh layerSubMesh = subMeshes[i];
			if (layerSubMesh.finalized && !layerSubMesh.disabled)
			{
				Graphics.DrawMesh(layerSubMesh.mesh, Matrix4x4.identity, layerSubMesh.material, layerSubMesh.renderLayer);
			}
		}
	}

	public abstract void Regenerate();

	protected void ClearSubMeshes(MeshParts parts)
	{
		foreach (LayerSubMesh subMesh in subMeshes)
		{
			subMesh.Clear(parts);
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
