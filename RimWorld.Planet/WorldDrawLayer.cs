using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public abstract class WorldDrawLayer : WorldDrawLayerBase
{
	public PlanetLayer planetLayer;

	private List<MeshCollider> meshCollidersInOrder;

	private List<List<int>> triangleIndexToTileID;

	public override bool Visible
	{
		get
		{
			if (base.Visible)
			{
				if (PlanetLayer.Selected != planetLayer)
				{
					return VisibleWhenLayerNotSelected;
				}
				return true;
			}
			return false;
		}
	}

	public override Vector3 Position
	{
		get
		{
			if (CameraIsOrigin)
			{
				return Find.WorldCameraDriver.CameraPosition;
			}
			return planetLayer.Origin;
		}
	}

	public bool Raycastable
	{
		get
		{
			if (!meshCollidersInOrder.NullOrEmpty())
			{
				return PlanetLayer.Selected == planetLayer;
			}
			return false;
		}
	}

	public void Initialize(PlanetLayer layer)
	{
		planetLayer = layer;
	}

	public override IEnumerable Regenerate()
	{
		triangleIndexToTileID?.Clear();
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
	}

	protected void AppendRaycastableTriangle(int subMeshIndex, int tileIndex)
	{
		if (triangleIndexToTileID == null)
		{
			triangleIndexToTileID = new List<List<int>>();
		}
		while (triangleIndexToTileID.Count < subMeshes.Count)
		{
			triangleIndexToTileID.Add(new List<int>());
		}
		triangleIndexToTileID[subMeshIndex].Add(tileIndex);
	}

	public bool TryGetTileFromRayHit(RaycastHit hit, out PlanetTile id)
	{
		id = PlanetTile.Invalid;
		for (int i = 0; i < meshCollidersInOrder.Count; i++)
		{
			if (meshCollidersInOrder[i] == hit.collider)
			{
				id = new PlanetTile(triangleIndexToTileID[i][hit.triangleIndex], planetLayer);
				return true;
			}
		}
		return false;
	}

	protected IEnumerable RegenerateWorldMeshColliders()
	{
		if (meshCollidersInOrder == null)
		{
			meshCollidersInOrder = new List<MeshCollider>();
		}
		else
		{
			meshCollidersInOrder.Clear();
		}
		GameObject gameObject = WorldTerrainColliderManager.Get(planetLayer, WorldCameraManager.WorldLayer);
		gameObject.transform.position = Position;
		MeshCollider[] components = gameObject.GetComponents<MeshCollider>();
		for (int i = 0; i < components.Length; i++)
		{
			Object.Destroy(components[i]);
		}
		for (int j = 0; j < subMeshes.Count; j++)
		{
			MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
			meshCollider.sharedMesh = subMeshes[j].mesh;
			meshCollidersInOrder.Add(meshCollider);
			yield return null;
		}
	}
}
