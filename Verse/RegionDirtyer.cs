using System.Collections.Generic;

namespace Verse
{
	public class RegionDirtyer
	{
		private Map map;

		private List<IntVec3> dirtyCells = new List<IntVec3>();

		private List<Region> regionsToDirty = new List<Region>();

		public bool AnyDirty => dirtyCells.Count > 0;

		public List<IntVec3> DirtyCells => dirtyCells;

		public RegionDirtyer(Map map)
		{
			this.map = map;
		}

		internal void Notify_WalkabilityChanged(IntVec3 c)
		{
			regionsToDirty.Clear();
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
				if (c2.InBounds(map))
				{
					Region regionAt_NoRebuild_InvalidAllowed = map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c2);
					if (regionAt_NoRebuild_InvalidAllowed != null && regionAt_NoRebuild_InvalidAllowed.valid)
					{
						map.temperatureCache.TryCacheRegionTempInfo(c, regionAt_NoRebuild_InvalidAllowed);
						regionsToDirty.Add(regionAt_NoRebuild_InvalidAllowed);
					}
				}
			}
			for (int j = 0; j < regionsToDirty.Count; j++)
			{
				SetRegionDirty(regionsToDirty[j]);
			}
			regionsToDirty.Clear();
			if (c.Walkable(map) && !dirtyCells.Contains(c))
			{
				dirtyCells.Add(c);
			}
		}

		internal void Notify_ThingAffectingRegionsSpawned(Thing b)
		{
			regionsToDirty.Clear();
			foreach (IntVec3 item in b.OccupiedRect().ExpandedBy(1).ClipInsideMap(b.Map))
			{
				Region validRegionAt_NoRebuild = b.Map.regionGrid.GetValidRegionAt_NoRebuild(item);
				if (validRegionAt_NoRebuild != null)
				{
					b.Map.temperatureCache.TryCacheRegionTempInfo(item, validRegionAt_NoRebuild);
					regionsToDirty.Add(validRegionAt_NoRebuild);
				}
			}
			for (int i = 0; i < regionsToDirty.Count; i++)
			{
				SetRegionDirty(regionsToDirty[i]);
			}
			regionsToDirty.Clear();
		}

		internal void Notify_ThingAffectingRegionsDespawned(Thing b)
		{
			regionsToDirty.Clear();
			Region validRegionAt_NoRebuild = map.regionGrid.GetValidRegionAt_NoRebuild(b.Position);
			if (validRegionAt_NoRebuild != null)
			{
				map.temperatureCache.TryCacheRegionTempInfo(b.Position, validRegionAt_NoRebuild);
				regionsToDirty.Add(validRegionAt_NoRebuild);
			}
			foreach (IntVec3 item2 in GenAdj.CellsAdjacent8Way(b))
			{
				if (item2.InBounds(map))
				{
					Region validRegionAt_NoRebuild2 = map.regionGrid.GetValidRegionAt_NoRebuild(item2);
					if (validRegionAt_NoRebuild2 != null)
					{
						map.temperatureCache.TryCacheRegionTempInfo(item2, validRegionAt_NoRebuild2);
						regionsToDirty.Add(validRegionAt_NoRebuild2);
					}
				}
			}
			for (int i = 0; i < regionsToDirty.Count; i++)
			{
				SetRegionDirty(regionsToDirty[i]);
			}
			regionsToDirty.Clear();
			if (b.def.size.x == 1 && b.def.size.z == 1)
			{
				dirtyCells.Add(b.Position);
				return;
			}
			CellRect cellRect = b.OccupiedRect();
			for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
			{
				for (int k = cellRect.minX; k <= cellRect.maxX; k++)
				{
					IntVec3 item = new IntVec3(k, 0, j);
					dirtyCells.Add(item);
				}
			}
		}

		internal void SetAllClean()
		{
			for (int i = 0; i < dirtyCells.Count; i++)
			{
				map.temperatureCache.ResetCachedCellInfo(dirtyCells[i]);
			}
			dirtyCells.Clear();
		}

		private void SetRegionDirty(Region reg, bool addCellsToDirtyCells = true)
		{
			if (!reg.valid)
			{
				return;
			}
			reg.valid = false;
			reg.Room = null;
			for (int i = 0; i < reg.links.Count; i++)
			{
				reg.links[i].Deregister(reg);
			}
			reg.links.Clear();
			if (!addCellsToDirtyCells)
			{
				return;
			}
			foreach (IntVec3 cell in reg.Cells)
			{
				dirtyCells.Add(cell);
				if (DebugViewSettings.drawRegionDirties)
				{
					map.debugDrawer.FlashCell(cell);
				}
			}
		}

		internal void SetAllDirty()
		{
			dirtyCells.Clear();
			foreach (IntVec3 item in map)
			{
				dirtyCells.Add(item);
			}
			foreach (Region item2 in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
			{
				SetRegionDirty(item2, addCellsToDirtyCells: false);
			}
		}
	}
}
