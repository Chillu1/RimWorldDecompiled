using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SectionLayer_GravshipMask : SectionLayer
{
	public enum MaskOverrideMode
	{
		None,
		Shadow,
		Gravship
	}

	public static Building_GravEngine Engine { get; set; }

	public static MaskOverrideMode OverrideMode { get; set; }

	public SectionLayer_GravshipMask(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.None;
	}

	public static void ResetStaticData()
	{
		Engine = null;
		OverrideMode = MaskOverrideMode.None;
	}

	public static LayerSubMesh BakeGravshipShadowMask(Map map, Material mat, IEnumerable<IntVec3> cells)
	{
		LayerSubMesh layerSubMesh = MapDrawLayer.CreateFreeSubMesh(mat);
		RegenerateGravshipShadowMask(map, layerSubMesh, cells);
		if (layerSubMesh.verts.Count > 0)
		{
			layerSubMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
		return layerSubMesh;
	}

	public static bool IsValidSubstructure(IntVec3 c)
	{
		if (Engine != null && Engine.Spawned && Engine.ValidSubstructureNoRegen != null)
		{
			return Engine.ValidSubstructureNoRegen.Contains(c);
		}
		return false;
	}

	public static LayerSubMesh BakeDummyShadowMask(Material mat, Vector3 origin, float width, float height)
	{
		LayerSubMesh layerSubMesh = MapDrawLayer.CreateFreeSubMesh(mat);
		Vector3 vector = new Vector3(width * 0.5f, 0f, height * 0.5f);
		layerSubMesh.verts.Add(origin - vector);
		layerSubMesh.verts.Add(origin + new Vector3(0f - vector.x, 0f, vector.z));
		layerSubMesh.verts.Add(origin + vector);
		layerSubMesh.verts.Add(origin + new Vector3(vector.x, 0f, 0f - vector.z));
		int count = layerSubMesh.verts.Count;
		layerSubMesh.tris.Add(count - 4);
		layerSubMesh.tris.Add(count - 3);
		layerSubMesh.tris.Add(count - 2);
		layerSubMesh.tris.Add(count - 4);
		layerSubMesh.tris.Add(count - 2);
		layerSubMesh.tris.Add(count - 1);
		layerSubMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		return layerSubMesh;
	}

	private void RegenerateGravshipMask(LayerSubMesh subMesh)
	{
		Building_GravEngine engine = Engine;
		if (engine == null || !engine.Spawned)
		{
			return;
		}
		HashSet<IntVec3> validSubstructure = Engine.ValidSubstructure;
		foreach (IntVec3 item in section.CellRect)
		{
			if (validSubstructure.Contains(item))
			{
				SectionLayer_IndoorMask.AppendQuadToMesh(subMesh, item.x, item.z, 0f);
			}
		}
	}

	private static void RegenerateGravshipShadowMask(Map map, LayerSubMesh subMesh, IEnumerable<IntVec3> cells)
	{
		string defName = TerrainDefOf.Space.defName;
		foreach (IntVec3 cell in cells)
		{
			if (cell.InBounds(map) && map.terrainGrid.TerrainAt(cell).defName != defName)
			{
				SectionLayer_IndoorMask.AppendQuadToMesh(subMesh, cell.x, cell.z, 0f);
			}
		}
	}

	public override void Regenerate()
	{
		LayerSubMesh subMesh = GetSubMesh(MatBases.GravshipMask);
		subMesh.Clear(MeshParts.All);
		switch (OverrideMode)
		{
		case MaskOverrideMode.Shadow:
			RegenerateGravshipShadowMask(base.Map, subMesh, section.CellRect);
			break;
		case MaskOverrideMode.Gravship:
			RegenerateGravshipMask(subMesh);
			break;
		case MaskOverrideMode.None:
			if (WorldComponent_GravshipController.CutsceneInProgress || Find.GravshipController.IsGravshipTravelling)
			{
				RegenerateGravshipShadowMask(base.Map, subMesh, section.CellRect);
			}
			else
			{
				RegenerateGravshipMask(subMesh);
			}
			break;
		}
		if (subMesh.verts.Count > 0)
		{
			subMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
	}
}
