using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SubstructureGrid
{
	private Map map;

	private CellBoolDrawer drawerInt;

	private List<Building_GravEngine> tmpEngines = new List<Building_GravEngine>();

	private int lastGotEnginesTick = -1;

	private static readonly List<IntVec3> tmpFootprintCells = new List<IntVec3>();

	private static readonly HashSet<IntVec3> tmpFootprintCellsHashSet = new HashSet<IntVec3>();

	private static readonly List<IntVec3> tmpGravEngineCells = new List<IntVec3>();

	private static readonly HashSet<IntVec3> tmpGravEngineCellsHashSet = new HashSet<IntVec3>();

	private static int lastUpdateFrame = -1;

	private static int lastGUIFrame = -1;

	private static readonly List<IntVec3> tmpExtraFootprintCells = new List<IntVec3>();

	public CellBoolDrawer Drawer => drawerInt ?? (drawerInt = new CellBoolDrawer(CellBoolDrawerGetBoolInt, CellBoolDrawerColorInt, CellBoolDrawerGetExtraColorInt, map.Size.x, map.Size.z, 3700));

	public SubstructureGrid(Map map)
	{
		this.map = map;
	}

	private Color CellBoolDrawerColorInt()
	{
		return Color.white;
	}

	private bool CellBoolDrawerGetBoolInt(int index)
	{
		return map.terrainGrid.FoundationAt(index)?.IsSubstructure ?? false;
	}

	private Color CellBoolDrawerGetExtraColorInt(int index)
	{
		if (Find.TickManager.TicksGame != lastGotEnginesTick)
		{
			tmpEngines.Clear();
			tmpEngines.AddRange(map.listerBuildings.AllBuildingsColonistOfClass<Building_GravEngine>());
			lastGotEnginesTick = Find.TickManager.TicksGame;
		}
		TerrainDef terrainDef = map.terrainGrid.FoundationAt(index);
		if (terrainDef != null && terrainDef.IsSubstructure)
		{
			foreach (Building_GravEngine tmpEngine in tmpEngines)
			{
				if (tmpEngine.ValidSubstructureAt(map.cellIndices.IndexToCell(index)))
				{
					return GravshipUtility.ConnectedSubstructureColor;
				}
			}
		}
		return GravshipUtility.DisconnectedSubstructureColor;
	}

	public void DrawSubstructureGrid()
	{
		if (GravshipUtility.ShowConnectedSubstructure && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
		}
		Drawer.CellBoolDrawerUpdate();
	}

	public static void DrawSubstructureFootprint(List<IntVec3> extraCells = null, Thing thingToExclude = null)
	{
		if (Time.frameCount == lastUpdateFrame)
		{
			return;
		}
		lastUpdateFrame = Time.frameCount;
		foreach (Thing item in Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
		{
			tmpFootprintCells.Clear();
			tmpFootprintCellsHashSet.Clear();
			tmpGravEngineCells.Clear();
			tmpGravEngineCellsHashSet.Clear();
			Building_GravEngine building_GravEngine = item as Building_GravEngine;
			if (building_GravEngine != thingToExclude)
			{
				CompSubstructureFootprint compSubstructureFootprint = building_GravEngine.TryGetComp<CompSubstructureFootprint>();
				tmpGravEngineCells.AddRange(GenRadial.RadialCellsAround(building_GravEngine.Position, compSubstructureFootprint.Props.radius, useCenter: true));
				tmpGravEngineCellsHashSet.AddRange(tmpGravEngineCells);
				GenDraw.DrawFieldEdges(tmpGravEngineCells, GravshipUtility.ConnectedSubstructureColor);
			}
			foreach (Thing item2 in building_GravEngine.AffectedByFacilities.LinkedFacilitiesListForReading)
			{
				if ((thingToExclude != null && item2 == thingToExclude) || !item2.TryGetComp(out CompSubstructureFootprint comp) || !comp.Valid)
				{
					continue;
				}
				foreach (IntVec3 item3 in GenRadial.RadialCellsAround(item2.Position, comp.Props.radius, useCenter: true))
				{
					if (item3.InBounds(Find.CurrentMap) && !item3.Fogged(Find.CurrentMap) && !tmpGravEngineCellsHashSet.Contains(item3) && !tmpFootprintCellsHashSet.Contains(item3))
					{
						tmpFootprintCells.Add(item3);
						tmpFootprintCellsHashSet.Add(item3);
					}
				}
			}
			if (extraCells != null)
			{
				foreach (IntVec3 extraCell in extraCells)
				{
					if (extraCell.InBounds(Find.CurrentMap) && !extraCell.Fogged(Find.CurrentMap) && !tmpGravEngineCellsHashSet.Contains(extraCell) && !tmpFootprintCellsHashSet.Contains(extraCell))
					{
						tmpFootprintCells.Add(extraCell);
						tmpFootprintCellsHashSet.Add(extraCell);
					}
				}
			}
			List<IntVec3> cells = tmpFootprintCells;
			Color connectedSubstructureColor = GravshipUtility.ConnectedSubstructureColor;
			HashSet<IntVec3> ignoreBorderCells = tmpGravEngineCellsHashSet;
			GenDraw.DrawFieldEdges(cells, connectedSubstructureColor, null, ignoreBorderCells);
		}
	}

	public static void DrawSubstructureCountOnGUI()
	{
		if (Time.frameCount == lastGUIFrame)
		{
			return;
		}
		foreach (Thing item in Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
		{
			Building_GravEngine obj = item as Building_GravEngine;
			int count = obj.AllConnectedSubstructure.Count;
			float statValue = obj.GetStatValue(StatDefOf.SubstructureSupport);
			Widgets.DrawStringOnMap(obj.DrawPos.MapToUIPosition(), $"{count}/{statValue}", Color.white);
		}
	}

	public static void DrawSubstructureFootprintWithExtra(CompProperties_SubstructureFootprint footprint, IntVec3 center, Thing thing)
	{
		tmpExtraFootprintCells.Clear();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(center, footprint.radius, useCenter: true))
		{
			if (item.InBounds(Find.CurrentMap) && !item.Fogged(Find.CurrentMap))
			{
				tmpExtraFootprintCells.Add(item);
			}
		}
		DrawSubstructureFootprint(tmpExtraFootprintCells, thing);
	}

	public void MarkDirty()
	{
		foreach (Building_GravEngine item in map.listerBuildings.AllBuildingsColonistOfClass<Building_GravEngine>())
		{
			item.substructureDirty = true;
		}
		Drawer.SetDirty();
	}
}
