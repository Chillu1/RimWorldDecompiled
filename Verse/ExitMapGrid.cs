using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public sealed class ExitMapGrid : ICellBoolGiver
{
	private Map map;

	private bool dirty = true;

	private BoolGrid exitMapGrid;

	private CellBoolDrawer drawerInt;

	private const int MaxDistToEdge = 2;

	[Unsaved(false)]
	private bool mapUsesExitGrid;

	[Unsaved(false)]
	private int lastCheckedMapTick = -99999;

	public bool MapUsesExitGrid
	{
		get
		{
			if (Find.TickManager.TicksGame == lastCheckedMapTick)
			{
				return mapUsesExitGrid;
			}
			lastCheckedMapTick = Find.TickManager.TicksGame;
			mapUsesExitGrid = MapUsesExitGridNow;
			return mapUsesExitGrid;
		}
	}

	private bool MapUsesExitGridNow
	{
		get
		{
			if (map.IsPlayerHome)
			{
				return false;
			}
			if (map.IsPocketMap)
			{
				return false;
			}
			if (map.Biome.inVacuum)
			{
				return false;
			}
			if (map.Parent is CaravansBattlefield caravansBattlefield && caravansBattlefield.def.blockExitGridUntilBattleIsWon && !caravansBattlefield.WonBattle)
			{
				return false;
			}
			FormCaravanComp formCaravanComp = map.Parent?.GetComponent<FormCaravanComp>();
			if (formCaravanComp != null && formCaravanComp.CanFormOrReformCaravanNow)
			{
				return false;
			}
			return true;
		}
	}

	public CellBoolDrawer Drawer
	{
		get
		{
			if (!MapUsesExitGrid)
			{
				return null;
			}
			if (dirty)
			{
				Rebuild();
			}
			if (drawerInt == null)
			{
				drawerInt = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3690);
			}
			return drawerInt;
		}
	}

	public BoolGrid Grid
	{
		get
		{
			if (!MapUsesExitGrid)
			{
				return null;
			}
			if (dirty)
			{
				Rebuild();
			}
			return exitMapGrid;
		}
	}

	public Color Color => new Color(0.35f, 1f, 0.35f, 0.12f);

	public ExitMapGrid(Map map)
	{
		this.map = map;
	}

	public bool GetCellBool(int index)
	{
		if (Grid[index])
		{
			return !map.fogGrid.IsFogged(index);
		}
		return false;
	}

	public Color GetCellExtraColor(int index)
	{
		return Color.white;
	}

	public bool IsExitCell(IntVec3 c)
	{
		if (!MapUsesExitGrid)
		{
			return false;
		}
		if (!c.InBounds(map))
		{
			return false;
		}
		return Grid[c];
	}

	public void ExitMapGridUpdate()
	{
		if (MapUsesExitGrid && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
			Drawer.CellBoolDrawerUpdate();
		}
	}

	public void Notify_LOSBlockerSpawned()
	{
		dirty = true;
	}

	public void Notify_LOSBlockerDespawned()
	{
		dirty = true;
	}

	private void Rebuild()
	{
		dirty = false;
		if (exitMapGrid == null)
		{
			exitMapGrid = new BoolGrid(map);
		}
		else
		{
			exitMapGrid.Clear();
		}
		CellRect cellRect = CellRect.WholeMap(map);
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				if (i > 1 && i < cellRect.maxZ - 2 + 1 && j > 1 && j < cellRect.maxX - 2 + 1)
				{
					j = cellRect.maxX - 2 + 1;
				}
				IntVec3 intVec = new IntVec3(j, 0, i);
				if (IsGoodExitCell(intVec))
				{
					exitMapGrid[intVec] = true;
				}
			}
		}
		if (drawerInt != null)
		{
			drawerInt.SetDirty();
		}
	}

	private bool IsGoodExitCell(IntVec3 cell)
	{
		if (!cell.CanBeSeenOver(map))
		{
			return false;
		}
		int num = GenRadial.NumCellsInRadius(2f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = cell + GenRadial.RadialPattern[i];
			if (intVec.InBounds(map) && intVec.OnEdge(map) && intVec.CanBeSeenOverFast(map) && GenSight.LineOfSight(cell, intVec, map))
			{
				return true;
			}
		}
		return false;
	}
}
