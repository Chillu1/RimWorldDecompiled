using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class MapDrawer
{
	private readonly Map map;

	private Section[,] sections;

	private List<MapDrawLayer> global;

	private ulong globalDirtyFlags;

	private IntVec2 SectionCount => new IntVec2
	{
		x = Mathf.CeilToInt((float)map.Size.x / 17f),
		z = Mathf.CeilToInt((float)map.Size.z / 17f)
	};

	private CellRect VisibleSections
	{
		get
		{
			CellRect viewRect = ViewRect;
			IntVec2 intVec = SectionCoordsAt(viewRect.Min);
			IntVec2 intVec2 = SectionCoordsAt(viewRect.Max);
			if (intVec2.x < intVec.x || intVec2.z < intVec.z)
			{
				return CellRect.Empty;
			}
			return CellRect.FromLimits(intVec.x, intVec.z, intVec2.x, intVec2.z);
		}
	}

	private CellRect ViewRect => Find.CameraDriver.CurrentViewRect.ExpandedBy(1).ClipInsideMap(map);

	public List<MapDrawLayer> GlobalLayers => global;

	public MapDrawer(Map map)
	{
		this.map = map;
	}

	public T GetGlobalLayer<T>() where T : MapDrawLayer
	{
		for (int i = 0; i < global.Count; i++)
		{
			if (global[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void MapMeshDirty(IntVec3 loc, ulong dirtyFlags)
	{
		bool regenAdjacentCells = (dirtyFlags & ((ulong)MapMeshFlagDefOf.FogOfWar | (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Roofs)) != 0;
		MapMeshDirty(loc, dirtyFlags, regenAdjacentCells, regenAdjacentSections: false);
	}

	public void MapMeshDirty(IntVec3 loc, ulong dirtyFlags, bool regenAdjacentCells, bool regenAdjacentSections)
	{
		if (Current.ProgramState != ProgramState.Playing || sections == null)
		{
			return;
		}
		SectionAt(loc).dirtyFlags |= dirtyFlags;
		globalDirtyFlags |= dirtyFlags;
		if (regenAdjacentCells)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = loc + GenAdj.AdjacentCells[i];
				if (intVec.InBounds(map))
				{
					SectionAt(intVec).dirtyFlags |= dirtyFlags;
				}
			}
		}
		if (!regenAdjacentSections)
		{
			return;
		}
		IntVec2 intVec2 = SectionCoordsAt(loc);
		for (int j = 0; j < 8; j++)
		{
			IntVec3 intVec3 = GenAdj.AdjacentCells[j];
			IntVec2 intVec4 = intVec2 + new IntVec2(intVec3.x, intVec3.z);
			IntVec2 sectionCount = SectionCount;
			if (intVec4.x >= 0 && intVec4.z >= 0 && intVec4.x <= sectionCount.x - 1 && intVec4.z <= sectionCount.z - 1)
			{
				sections[intVec4.x, intVec4.z].dirtyFlags |= dirtyFlags;
			}
		}
	}

	public void MapMeshDrawerUpdate_First()
	{
		if (globalDirtyFlags != 0L)
		{
			for (int i = 0; i < global.Count; i++)
			{
				MapDrawLayer mapDrawLayer = global[i];
				mapDrawLayer.Dirty = mapDrawLayer.Dirty || (globalDirtyFlags & mapDrawLayer.relevantChangeTypes) != 0;
				if (mapDrawLayer.Dirty)
				{
					mapDrawLayer.Regenerate();
				}
			}
		}
		CellRect viewRect = ViewRect;
		bool flag = false;
		Section[,] array = sections;
		int upperBound = array.GetUpperBound(0);
		int upperBound2 = array.GetUpperBound(1);
		for (int j = array.GetLowerBound(0); j <= upperBound; j++)
		{
			for (int k = array.GetLowerBound(1); k <= upperBound2; k++)
			{
				if (array[j, k].TryUpdate(viewRect))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			return;
		}
		for (int l = 0; l < SectionCount.x; l++)
		{
			for (int m = 0; m < SectionCount.z; m++)
			{
				if (sections[l, m].TryUpdate(viewRect))
				{
					return;
				}
			}
		}
	}

	public void DrawMapMesh()
	{
		CellRect view = ViewRect;
		if (WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			view = view.Encapsulate(WorldComponent_GravshipController.GravshipRenderBounds);
		}
		for (int i = 0; i < global.Count; i++)
		{
			MapDrawLayer mapDrawLayer = global[i];
			if (mapDrawLayer.Visible)
			{
				if (mapDrawLayer.Dirty)
				{
					mapDrawLayer.Regenerate();
					mapDrawLayer.RefreshSubMeshBounds();
				}
				mapDrawLayer.DrawLayer();
			}
		}
		for (int j = 0; j < SectionCount.x; j++)
		{
			for (int k = 0; k < SectionCount.z; k++)
			{
				Section section = sections[j, k];
				if (view.Overlaps(section.Bounds))
				{
					section.DrawSection();
				}
				else
				{
					section.DrawDynamicSections(view);
				}
			}
		}
	}

	private IntVec2 SectionCoordsAt(IntVec3 loc)
	{
		return new IntVec2(Mathf.FloorToInt(loc.x / 17), Mathf.FloorToInt(loc.z / 17));
	}

	public Section SectionAt(IntVec3 loc)
	{
		IntVec2 intVec = SectionCoordsAt(loc);
		return sections[intVec.x, intVec.z];
	}

	public void RegenerateLayerNow(Type type)
	{
		EnsureGlobalLayersInitialized();
		if (sections == null)
		{
			sections = new Section[SectionCount.x, SectionCount.z];
		}
		for (int i = 0; i < global.Count; i++)
		{
			MapDrawLayer mapDrawLayer = global[i];
			if (mapDrawLayer.GetType() == type && mapDrawLayer.Visible)
			{
				global[i].Regenerate();
				mapDrawLayer.RefreshSubMeshBounds();
			}
		}
		for (int j = 0; j < SectionCount.x; j++)
		{
			for (int k = 0; k < SectionCount.z; k++)
			{
				Section[,] array = sections;
				int num = j;
				int num2 = k;
				if (array[num, num2] == null)
				{
					array[num, num2] = new Section(new IntVec3(j, 0, k), map);
				}
				sections[j, k].RegenerateSingleLayer(sections[j, k].GetLayer(type));
			}
		}
	}

	public void RegenerateEverythingNow()
	{
		EnsureGlobalLayersInitialized();
		if (sections == null)
		{
			sections = new Section[SectionCount.x, SectionCount.z];
		}
		for (int i = 0; i < global.Count; i++)
		{
			MapDrawLayer mapDrawLayer = global[i];
			if (mapDrawLayer.Visible)
			{
				mapDrawLayer.Regenerate();
				mapDrawLayer.RefreshSubMeshBounds();
			}
		}
		for (int j = 0; j < SectionCount.x; j++)
		{
			for (int k = 0; k < SectionCount.z; k++)
			{
				Section[,] array = sections;
				int num = j;
				int num2 = k;
				if (array[num, num2] == null)
				{
					array[num, num2] = new Section(new IntVec3(j, 0, k), map);
				}
				sections[j, k].RegenerateAllLayers();
			}
		}
	}

	private void EnsureGlobalLayersInitialized()
	{
		if (global != null)
		{
			return;
		}
		global = new List<MapDrawLayer>();
		foreach (Type item in typeof(MapDrawLayer).AllSubclassesNonAbstract())
		{
			if (!typeof(SectionLayer).IsAssignableFrom(item))
			{
				global.Add((MapDrawLayer)Activator.CreateInstance(item, map));
			}
		}
	}

	public void WholeMapChanged(ulong change)
	{
		globalDirtyFlags |= change;
		for (int i = 0; i < SectionCount.x; i++)
		{
			for (int j = 0; j < SectionCount.z; j++)
			{
				sections[i, j].dirtyFlags |= change;
			}
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < SectionCount.x; i++)
		{
			for (int j = 0; j < SectionCount.z; j++)
			{
				sections[i, j].Dispose();
			}
		}
		sections = null;
		foreach (MapDrawLayer item in global)
		{
			item.Dispose();
		}
	}
}
