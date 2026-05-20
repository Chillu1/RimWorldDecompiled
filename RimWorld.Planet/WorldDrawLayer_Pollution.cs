using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_Pollution : WorldDrawLayer
{
	private const int TilesPerSubMesh = 500;

	private const float ScaleUVFactor = 0.1f;

	private static readonly Color DefaultTileColor = Color.white;

	private static readonly Color BordersUnpollutedTileColor = new Color(1f, 1f, 1f, 0.4f);

	private readonly List<Vector3> verts = new List<Vector3>();

	private readonly Dictionary<int, List<LayerSubMesh>> subMeshesByRegion = new Dictionary<int, List<LayerSubMesh>>();

	private readonly Queue<int> regionsToRegenerate = new Queue<int>();

	private Material lightPollution;

	private Material moderatePollution;

	private Material extemePollution;

	private readonly List<PlanetTile> tmpNeighbors = new List<PlanetTile>();

	private readonly HashSet<Vector3> tmpBordersUnpollutedVerts = new HashSet<Vector3>();

	private readonly List<Vector3> tmpVerts = new List<Vector3>();

	private static readonly List<PlanetTile> TmpChangedNeighbours = new List<PlanetTile>();

	private Material LightPollution
	{
		get
		{
			if (lightPollution == null)
			{
				lightPollution = MaterialPool.MatFrom("World/Pollution/Light", ShaderDatabase.WorldOverlayTransparentLitPollution, 3510);
			}
			return lightPollution;
		}
	}

	private Material ModeratePollution
	{
		get
		{
			if (moderatePollution == null)
			{
				moderatePollution = MaterialPool.MatFrom("World/Pollution/Moderate", ShaderDatabase.WorldOverlayTransparentLitPollution, 3510);
			}
			return moderatePollution;
		}
	}

	private Material ExtremePollution
	{
		get
		{
			if (extemePollution == null)
			{
				extemePollution = MaterialPool.MatFrom("World/Pollution/Extreme", ShaderDatabase.WorldOverlayTransparentLitPollution, 3510);
			}
			return extemePollution;
		}
	}

	private int GetRegionIdForTile(PlanetTile tile)
	{
		return Mathf.FloorToInt((float)tile.tileId / 500f);
	}

	public List<LayerSubMesh> GetSubMeshesForRegion(int regionId)
	{
		if (!subMeshesByRegion.ContainsKey(regionId))
		{
			subMeshesByRegion[regionId] = new List<LayerSubMesh>();
		}
		return subMeshesByRegion[regionId];
	}

	public LayerSubMesh GetSubMeshForMaterialAndRegion(Material material, int regionId)
	{
		List<LayerSubMesh> subMeshesForRegion = GetSubMeshesForRegion(regionId);
		for (int i = 0; i < subMeshesForRegion.Count; i++)
		{
			if (subMeshesForRegion[i].material == material)
			{
				return subMeshesForRegion[i];
			}
		}
		Mesh mesh = new Mesh();
		if (UnityData.isEditor)
		{
			mesh.name = "WorldLayerSubMesh_" + GetType().Name + "_" + Find.World.info.seedString;
		}
		LayerSubMesh layerSubMesh = new LayerSubMesh(mesh, material);
		subMeshesForRegion.Add(layerSubMesh);
		subMeshes.Add(layerSubMesh);
		return layerSubMesh;
	}

	private void RegenerateRegion(int regionId)
	{
		List<LayerSubMesh> subMeshesForRegion = GetSubMeshesForRegion(regionId);
		for (int i = 0; i < subMeshesForRegion.Count; i++)
		{
			subMeshesForRegion[i].Clear(MeshParts.All);
		}
		int num = regionId * 500;
		int num2 = num + 500;
		for (int j = num; j < num2; j++)
		{
			PlanetTile tile = new PlanetTile(j, planetLayer);
			if (!Find.World.grid.InBounds(tile))
			{
				break;
			}
			TryAddMeshForTile(tile);
		}
		for (int k = 0; k < subMeshesForRegion.Count; k++)
		{
			if (subMeshesForRegion[k].verts.Count > 0)
			{
				subMeshesForRegion[k].FinalizeMesh(MeshParts.All);
			}
		}
	}

	public override IEnumerable Regenerate()
	{
		if (!ModsConfig.BiotechActive)
		{
			yield break;
		}
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		int pollutedMeshesPrinted = 0;
		verts.Clear();
		subMeshesByRegion.Clear();
		regionsToRegenerate.Clear();
		for (int i = 0; i < planetLayer.TilesCount; i++)
		{
			if (TryAddMeshForTile(planetLayer.PlanetTileForID(i)))
			{
				pollutedMeshesPrinted++;
				if (pollutedMeshesPrinted % 1000 == 0)
				{
					yield return null;
				}
			}
		}
		FinalizeMesh(MeshParts.All);
	}

	private bool TryAddMeshForTile(PlanetTile tile)
	{
		PollutionLevel pollution = tile.Tile.PollutionLevel();
		Material materialForTilePollution = GetMaterialForTilePollution(pollution);
		if (materialForTilePollution == null)
		{
			return false;
		}
		int regionIdForTile = GetRegionIdForTile(tile);
		LayerSubMesh subMeshForMaterialAndRegion = GetSubMeshForMaterialAndRegion(materialForTilePollution, regionIdForTile);
		Find.WorldGrid.GetTileVertices(tile, verts);
		Find.WorldGrid.GetTileNeighbors(tile, tmpNeighbors);
		int count = subMeshForMaterialAndRegion.verts.Count;
		tmpBordersUnpollutedVerts.Clear();
		tmpVerts.Clear();
		for (int i = 0; i < tmpNeighbors.Count; i++)
		{
			if (planetLayer[tmpNeighbors[i]].PollutionLevel() < PollutionLevel.Moderate)
			{
				Vector3 center = Find.WorldGrid.GetTileCenter(tmpNeighbors[i]);
				tmpVerts.AddRange(verts);
				tmpVerts.SortBy((Vector3 v) => Vector2.Distance(center, v));
				for (int num = 0; num < 2; num++)
				{
					tmpBordersUnpollutedVerts.Add(tmpVerts[num]);
				}
			}
		}
		int num2 = 0;
		for (int count2 = verts.Count; num2 < count2; num2++)
		{
			Vector3 vector = verts[num2] + verts[num2].normalized * 0.02f;
			subMeshForMaterialAndRegion.verts.Add(vector);
			subMeshForMaterialAndRegion.uvs.Add(vector * 0.1f);
			Color color = (tmpBordersUnpollutedVerts.Contains(verts[num2]) ? BordersUnpollutedTileColor : DefaultTileColor);
			subMeshForMaterialAndRegion.colors.Add(color);
			if (num2 < count2 - 2)
			{
				subMeshForMaterialAndRegion.tris.Add(count + num2 + 2);
				subMeshForMaterialAndRegion.tris.Add(count + num2 + 1);
				subMeshForMaterialAndRegion.tris.Add(count);
			}
		}
		tmpBordersUnpollutedVerts.Clear();
		tmpVerts.Clear();
		return true;
	}

	private Material GetMaterialForTilePollution(PollutionLevel pollution)
	{
		return pollution switch
		{
			PollutionLevel.Light => LightPollution, 
			PollutionLevel.Moderate => ModeratePollution, 
			PollutionLevel.Extreme => ExtremePollution, 
			_ => null, 
		};
	}

	public void Notify_TilePollutionChanged(PlanetTile tileId)
	{
		int regionIdForTile = GetRegionIdForTile(tileId);
		if (!regionsToRegenerate.Contains(regionIdForTile))
		{
			regionsToRegenerate.Enqueue(regionIdForTile);
		}
		Find.WorldGrid.GetTileNeighbors(tileId, TmpChangedNeighbours);
		for (int i = 0; i < TmpChangedNeighbours.Count; i++)
		{
			int regionIdForTile2 = GetRegionIdForTile(TmpChangedNeighbours[i]);
			if (!regionsToRegenerate.Contains(regionIdForTile2))
			{
				regionsToRegenerate.Enqueue(regionIdForTile2);
			}
		}
		TmpChangedNeighbours.Clear();
	}

	public override void Render()
	{
		if (regionsToRegenerate.Count > 0)
		{
			int regionId = regionsToRegenerate.Dequeue();
			RegenerateRegion(regionId);
		}
		base.Render();
	}
}
