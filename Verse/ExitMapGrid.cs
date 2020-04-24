using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public sealed class ExitMapGrid : ICellBoolGiver
	{
		private Map map;

		private bool dirty = true;

		private BoolGrid exitMapGrid;

		private CellBoolDrawer drawerInt;

		private const int MaxDistToEdge = 2;

		public bool MapUsesExitGrid
		{
			get
			{
				if (map.IsPlayerHome)
				{
					return false;
				}
				CaravansBattlefield caravansBattlefield = map.Parent as CaravansBattlefield;
				if (caravansBattlefield != null && caravansBattlefield.def.blockExitGridUntilBattleIsWon && !caravansBattlefield.WonBattle)
				{
					return false;
				}
				FormCaravanComp component = map.Parent.GetComponent<FormCaravanComp>();
				if (component != null && component.CanFormOrReformCaravanNow)
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
			return Grid[c];
		}

		public void ExitMapGridUpdate()
		{
			if (MapUsesExitGrid)
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
}
