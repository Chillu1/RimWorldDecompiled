using RimWorld;
using UnityEngine;

namespace Verse
{
	public sealed class RoofGrid : IExposable, ICellBoolGiver
	{
		private Map map;

		private RoofDef[] roofGrid;

		private CellBoolDrawer drawerInt;

		public CellBoolDrawer Drawer
		{
			get
			{
				if (drawerInt == null)
				{
					drawerInt = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3620);
				}
				return drawerInt;
			}
		}

		public Color Color => new Color(0.3f, 1f, 0.4f);

		public RoofGrid(Map map)
		{
			this.map = map;
			roofGrid = new RoofDef[map.cellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			MapExposeUtility.ExposeUshort(map, (IntVec3 c) => (ushort)((roofGrid[map.cellIndices.CellToIndex(c)] != null) ? roofGrid[map.cellIndices.CellToIndex(c)].shortHash : 0), delegate(IntVec3 c, ushort val)
			{
				SetRoof(c, DefDatabase<RoofDef>.GetByShortHash(val));
			}, "roofs");
		}

		public bool GetCellBool(int index)
		{
			if (roofGrid[index] != null)
			{
				return !map.fogGrid.IsFogged(index);
			}
			return false;
		}

		public Color GetCellExtraColor(int index)
		{
			if (RoofDefOf.RoofRockThick != null && roofGrid[index] == RoofDefOf.RoofRockThick)
			{
				return Color.gray;
			}
			return Color.white;
		}

		public bool Roofed(int index)
		{
			return roofGrid[index] != null;
		}

		public bool Roofed(int x, int z)
		{
			return roofGrid[map.cellIndices.CellToIndex(x, z)] != null;
		}

		public bool Roofed(IntVec3 c)
		{
			return roofGrid[map.cellIndices.CellToIndex(c)] != null;
		}

		public RoofDef RoofAt(int index)
		{
			return roofGrid[index];
		}

		public RoofDef RoofAt(IntVec3 c)
		{
			return roofGrid[map.cellIndices.CellToIndex(c)];
		}

		public RoofDef RoofAt(int x, int z)
		{
			return roofGrid[map.cellIndices.CellToIndex(x, z)];
		}

		public void SetRoof(IntVec3 c, RoofDef def)
		{
			if (roofGrid[map.cellIndices.CellToIndex(c)] != def)
			{
				roofGrid[map.cellIndices.CellToIndex(c)] = def;
				map.glowGrid.MarkGlowGridDirty(c);
				map.regionGrid.GetValidRegionAt_NoRebuild(c)?.Room.Notify_RoofChanged();
				if (drawerInt != null)
				{
					drawerInt.SetDirty();
				}
				map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Roofs);
			}
		}

		public void RoofGridUpdate()
		{
			if (Find.PlaySettings.showRoofOverlay)
			{
				Drawer.MarkForDraw();
			}
			Drawer.CellBoolDrawerUpdate();
		}
	}
}
