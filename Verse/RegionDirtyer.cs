using System.Collections.Generic;

namespace Verse;

public class RegionDirtyer
{
	private Map map;

	private HashSet<IntVec3> dirtyCells = new HashSet<IntVec3>();

	private List<Region> regionsToDirty = new List<Region>();

	public bool AnyDirty => dirtyCells.Count > 0;

	public HashSet<IntVec3> DirtyCells => dirtyCells;

	public RegionDirtyer(Map map)
	{
		this.map = map;
	}

	internal void Notify_WalkabilityChanged(IntVec3 c, bool newWalkability)
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
					map.TemperatureVacuumCache.TryCacheRegionTempInfo(c2, regionAt_NoRebuild_InvalidAllowed);
					regionsToDirty.Add(regionAt_NoRebuild_InvalidAllowed);
				}
			}
		}
		for (int j = 0; j < regionsToDirty.Count; j++)
		{
			SetRegionDirty(regionsToDirty[j]);
		}
		regionsToDirty.Clear();
		if (newWalkability)
		{
			dirtyCells.Add(c);
		}
	}

	internal void Notify_ThingAffectingRegionsSpawned(Thing b)
	{
		DirtyRegionForThing(b);
	}

	internal void Notify_ThingAffectingRegionsDespawned(Thing b)
	{
		DirtyRegionForThing(b);
	}

	internal void DirtyRegionForThing(Thing b)
	{
		regionsToDirty.Clear();
		foreach (IntVec3 item in b.OccupiedRect().ExpandedBy(1).ClipInsideMap(b.Map))
		{
			Region validRegionAt_NoRebuild = b.Map.regionGrid.GetValidRegionAt_NoRebuild(item);
			if (validRegionAt_NoRebuild != null)
			{
				b.Map.TemperatureVacuumCache.TryCacheRegionTempInfo(item, validRegionAt_NoRebuild);
				regionsToDirty.Add(validRegionAt_NoRebuild);
			}
		}
		for (int i = 0; i < regionsToDirty.Count; i++)
		{
			SetRegionDirty(regionsToDirty[i]);
		}
		regionsToDirty.Clear();
	}

	internal void SetAllClean()
	{
		foreach (IntVec3 dirtyCell in dirtyCells)
		{
			map.TemperatureVacuumCache.ResetCachedCellInfo(dirtyCell);
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
		reg.District = null;
		for (int i = 0; i < reg.links.Count; i++)
		{
			reg.links[i].Deregister(reg);
		}
		reg.links.Clear();
		reg.ListerThings.Clear();
		map.GetComponent<VacuumComponent>().SetDrawerDirty();
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
