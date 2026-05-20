using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class PollutionGrid : IExposable
{
	private const int LessonPollutionTicks = 200;

	private const int PerformantPollutionEffectThreshold = 10;

	public const int DissolutionEffecterDuration = 45;

	private BoolGrid grid;

	private Map map;

	private bool dirty;

	private CellBoolDrawer drawerInt;

	private List<IntVec3> allPollutableCells = new List<IntVec3>();

	private List<IntVec3> pollutedCellsThisTick = new List<IntVec3>();

	public int TotalPollution => grid.TrueCount;

	public List<IntVec3> AllPollutableCells
	{
		get
		{
			allPollutableCells.Clear();
			allPollutableCells.AddRange(map.AllCells.Where(EverPollutable));
			return allPollutableCells;
		}
	}

	public float TotalPollutionPercent => (float)grid.TrueCount / (float)AllPollutableCells.Count;

	public CellBoolDrawer Drawer
	{
		get
		{
			if (drawerInt == null)
			{
				drawerInt = new CellBoolDrawer(CellBoolDrawerGetBoolInt, CellBoolDrawerColorInt, CellBoolDrawerGetExtraColorInt, map.Size.x, map.Size.z, 3650);
			}
			return drawerInt;
		}
	}

	public PollutionGrid(Map map)
	{
		grid = new BoolGrid(map);
		this.map = map;
	}

	public bool IsPolluted(IntVec3 cell)
	{
		if (ModsConfig.BiotechActive && cell.InBounds(map))
		{
			return grid[cell];
		}
		return false;
	}

	public bool EverPollutable(IntVec3 cell)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice != null && !edifice.def.EverPollutable)
		{
			return false;
		}
		return cell.GetTerrain(map).canBePolluted;
	}

	public void SetPolluted(IntVec3 cell, bool isPolluted, bool silent = false)
	{
		if (ModLister.CheckBiotech("Set pollution") && cell.InBounds(map) && grid[cell] != isPolluted)
		{
			grid.Set(cell, isPolluted);
			dirty = true;
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Pollution, regenAdjacentCells: false, regenAdjacentSections: false);
			Drawer.SetDirty();
			map.fertilityGrid.Drawer.SetDirty();
			map.waterBodyTracker?.Notify_PollutionChanged(cell, isPolluted);
			if (!silent && isPolluted && MapGenerator.mapBeingGenerated != map && !pollutedCellsThisTick.Contains(cell))
			{
				pollutedCellsThisTick.Add(cell);
			}
		}
	}

	public bool CanPollute(IntVec3 c)
	{
		if (!EverPollutable(c))
		{
			return false;
		}
		return !IsPolluted(c);
	}

	public bool CanUnpollute(IntVec3 c)
	{
		return IsPolluted(c);
	}

	public void PollutionTick()
	{
		if (dirty)
		{
			if (map.Tile.Valid)
			{
				Find.WorldGrid[map.Tile].pollution = TotalPollutionPercent;
				Find.World.renderer.Notify_TilePollutionChanged(map.Tile);
			}
			dirty = false;
		}
		if (map.IsHashIntervalTick(200) && TotalPollution > 0)
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.PollutedTerrain, OpportunityType.Important);
		}
		if (pollutedCellsThisTick.Count > 0)
		{
			EffecterDef effecterDef = ((pollutedCellsThisTick.Count > 10) ? EffecterDefOf.CellPollution_Performant : EffecterDefOf.CellPollution);
			for (int i = 0; i < pollutedCellsThisTick.Count; i++)
			{
				IntVec3 intVec = pollutedCellsThisTick[i];
				Effecter eff = effecterDef.Spawn(intVec, map, Vector3.zero);
				map.effecterMaintainer.AddEffecterToMaintain(eff, intVec, 45);
			}
		}
		pollutedCellsThisTick.Clear();
	}

	public void PollutionGridUpdate()
	{
		if (Find.PlaySettings.showPollutionOverlay && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
		}
		Drawer.CellBoolDrawerUpdate();
	}

	private bool CellBoolDrawerGetBoolInt(int index)
	{
		IntVec3 intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
		if (!intVec.InBounds(map) || intVec.Filled(map) || intVec.Fogged(map))
		{
			return false;
		}
		if (!EverPollutable(intVec))
		{
			return false;
		}
		return IsPolluted(intVec);
	}

	private Color CellBoolDrawerColorInt()
	{
		return Color.white;
	}

	private Color CellBoolDrawerGetExtraColorInt(int index)
	{
		IntVec3 cell = CellIndicesUtility.IndexToCell(index, map.Size.x);
		if (!IsPolluted(cell))
		{
			return Color.white;
		}
		return Color.red;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref grid, "grid");
	}
}
