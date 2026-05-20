using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class TerrainGrid : IExposable
{
	private Map map;

	private TerrainDef[] tempGrid;

	public TerrainDef[] topGrid;

	public TerrainDef[] foundationGrid;

	private TerrainDef[] underGrid;

	public ColorDef[] colorGrid;

	private CellBoolDrawer drawerInt;

	private HashSet<IntVec3> waterCells;

	private Dictionary<(TerrainDef, bool, ColorDef), Material> terrainMatCache = new Dictionary<(TerrainDef, bool, ColorDef), Material>();

	private static readonly Color NoAffordanceColor = Color.red;

	public CellBoolDrawer Drawer
	{
		get
		{
			if (drawerInt == null)
			{
				drawerInt = new CellBoolDrawer(CellBoolDrawerGetBoolInt, CellBoolDrawerColorInt, CellBoolDrawerGetExtraColorInt, map.Size.x, map.Size.z, 3600);
			}
			return drawerInt;
		}
	}

	public bool AnyWaterCells => waterCells.Count > 0;

	public TerrainGrid(Map map)
	{
		this.map = map;
		ResetGrids();
	}

	public void ResetGrids()
	{
		topGrid = new TerrainDef[map.cellIndices.NumGridCells];
		underGrid = new TerrainDef[map.cellIndices.NumGridCells];
		foundationGrid = new TerrainDef[map.cellIndices.NumGridCells];
		tempGrid = new TerrainDef[map.cellIndices.NumGridCells];
		colorGrid = new ColorDef[map.cellIndices.NumGridCells];
		waterCells = new HashSet<IntVec3>();
	}

	public TerrainDef TerrainAt(int ind)
	{
		if (tempGrid[ind] != null)
		{
			return tempGrid[ind];
		}
		if (underGrid[ind] == null && foundationGrid[ind] != null)
		{
			return foundationGrid[ind];
		}
		if (topGrid[ind] == null)
		{
			return TerrainDefOf.Soil;
		}
		return topGrid[ind];
	}

	public TerrainDef TerrainAtIgnoreTemp(int ind)
	{
		if (underGrid[ind] == null && foundationGrid[ind] != null)
		{
			return foundationGrid[ind];
		}
		if (topGrid[ind] == null)
		{
			return TerrainDefOf.Soil;
		}
		return topGrid[ind];
	}

	public TerrainDef TerrainAt(IntVec3 c)
	{
		return TerrainAt(map.cellIndices.CellToIndex(c));
	}

	public TerrainDef BaseTerrainAt(IntVec3 c)
	{
		int num = map.cellIndices.CellToIndex(c);
		if (underGrid[num] != null)
		{
			return underGrid[num];
		}
		if (topGrid[num] == null)
		{
			return TerrainDefOf.Soil;
		}
		return topGrid[num];
	}

	public TerrainDef TopTerrainAt(int ind)
	{
		if (topGrid[ind] == null)
		{
			return TerrainDefOf.Soil;
		}
		return topGrid[ind];
	}

	public TerrainDef TopTerrainAt(IntVec3 c)
	{
		if (topGrid[map.cellIndices.CellToIndex(c)] == null)
		{
			return TerrainDefOf.Soil;
		}
		return topGrid[map.cellIndices.CellToIndex(c)];
	}

	public TerrainDef UnderTerrainAt(int ind)
	{
		return underGrid[ind];
	}

	public TerrainDef UnderTerrainAt(IntVec3 c)
	{
		return underGrid[map.cellIndices.CellToIndex(c)];
	}

	public TerrainDef FoundationAt(int ind)
	{
		return foundationGrid[ind];
	}

	public TerrainDef FoundationAt(IntVec3 c)
	{
		return foundationGrid[map.cellIndices.CellToIndex(c)];
	}

	public TerrainDef TempTerrainAt(int ind)
	{
		return tempGrid[ind];
	}

	public TerrainDef TempTerrainAt(IntVec3 c)
	{
		return tempGrid[map.cellIndices.CellToIndex(c)];
	}

	public ColorDef ColorAt(IntVec3 c)
	{
		return ColorAt(map.cellIndices.CellToIndex(c));
	}

	public bool WaterAt(IntVec3 c)
	{
		return waterCells.Contains(c);
	}

	public ColorDef ColorAt(int ind)
	{
		if (tempGrid[ind] != null)
		{
			return null;
		}
		return colorGrid[ind];
	}

	public Material GetMaterial(TerrainDef def, bool polluted, ColorDef color)
	{
		(TerrainDef, bool, ColorDef) key = (def, polluted, color);
		if (!terrainMatCache.ContainsKey(key))
		{
			Graphic graphic = (polluted ? def.graphicPolluted : def.graphic);
			if (color != null)
			{
				terrainMatCache[key] = graphic.GetColoredVersion(def.graphic.Shader, color.color, Color.white).MatSingle;
			}
			else
			{
				terrainMatCache[key] = (polluted ? def.DrawMatPolluted : def.DrawMatSingle);
			}
		}
		return terrainMatCache[key];
	}

	public void SetTerrain(IntVec3 c, TerrainDef newTerr)
	{
		if (newTerr == null)
		{
			Log.Error($"Tried to set terrain at {c} to null.");
			return;
		}
		if (!c.InBounds(map))
		{
			Log.Error($"Tried to set terrain in out of bounds cell {c} (bounds: {map.Size})");
			return;
		}
		if (newTerr.temporary)
		{
			SetTempTerrain(c, newTerr);
			return;
		}
		if (newTerr.isFoundation)
		{
			SetFoundation(c, newTerr);
			return;
		}
		int num = map.cellIndices.CellToIndex(c);
		if (newTerr.layerable)
		{
			if (underGrid[num] == null)
			{
				if (topGrid[num] != null && topGrid[num].passability != Traversability.Impassable)
				{
					underGrid[num] = topGrid[num];
				}
				else
				{
					underGrid[num] = map.generatorDef.defaultUnderGridTerrain ?? TerrainDefOf.Sand;
				}
			}
		}
		else
		{
			underGrid[num] = null;
		}
		TerrainDef oldTerr = null;
		if (topGrid[num] != null)
		{
			if (topGrid[num].glowRadius > 0f)
			{
				map.glowGrid.DeregisterTerrain(c);
			}
			if (topGrid[num].IsWater)
			{
				waterCells.Remove(c);
			}
			oldTerr = topGrid[num];
		}
		topGrid[num] = newTerr;
		colorGrid[num] = null;
		if (newTerr.glowRadius > 0f)
		{
			map.glowGrid.RegisterTerrain(c);
		}
		if (newTerr.IsWater)
		{
			waterCells.Add(c);
		}
		DoTerrainChangedEffects(c, oldTerr, newTerr);
	}

	public void SetUnderTerrain(IntVec3 c, TerrainDef newTerr)
	{
		if (!c.InBounds(map))
		{
			Log.Error($"Tried to set terrain out of bounds at {c}");
			return;
		}
		int num = map.cellIndices.CellToIndex(c);
		if (underGrid[num] != null && underGrid[num].glowRadius > 0f)
		{
			map.glowGrid.DeregisterTerrain(c);
		}
		underGrid[num] = newTerr;
		if (newTerr.glowRadius > 0f)
		{
			map.glowGrid.RegisterTerrain(c);
		}
		map.events.Notify_TerrainChanged(c);
	}

	public void RemoveTopLayer(IntVec3 c, bool doLeavings = true)
	{
		int num = map.cellIndices.CellToIndex(c);
		TerrainDef oldTerr = TerrainAt(c);
		if (tempGrid[num] != null)
		{
			RemoveTempTerrain(c, doLeavings);
			return;
		}
		if (foundationGrid[num] != null && underGrid[num] == null)
		{
			RemoveFoundation(c, doLeavings);
			return;
		}
		if (doLeavings)
		{
			GenLeaving.DoLeavingsFor(topGrid[num], c, map);
		}
		if (underGrid[num] != null)
		{
			if (topGrid[num].glowRadius > 0f)
			{
				map.glowGrid.DeregisterTerrain(c);
			}
			topGrid[num] = underGrid[num];
			underGrid[num] = null;
			colorGrid[num] = null;
			if (foundationGrid[num] != null && foundationGrid[num].glowRadius > 0f)
			{
				map.glowGrid.RegisterTerrain(c);
			}
			else if (topGrid[num].glowRadius > 0f)
			{
				map.glowGrid.RegisterTerrain(c);
			}
			DoTerrainChangedEffects(c, oldTerr, TerrainAt(c));
		}
	}

	public void SetFoundation(IntVec3 c, TerrainDef newTerr)
	{
		if (newTerr == null)
		{
			Log.Error($"Tried to set terrain at {c} to null.");
			return;
		}
		if (!newTerr.isFoundation)
		{
			Log.Error($"Tried to set foundation terrain at {c} to a type which is not a foundation ({newTerr.defName}).");
			return;
		}
		int num = map.cellIndices.CellToIndex(c);
		TerrainDef oldTerr = TerrainAt(num);
		if (TempTerrainAt(num) != null)
		{
			RemoveTempTerrain(c, doLeavings: false, preventDestroyEffects: true);
		}
		if (underGrid[num] != null)
		{
			Log.Error($"Tried to set foundation at {c} to {newTerr} but there is an under terrain there.");
			return;
		}
		if (topGrid[num] != null && topGrid[num].glowRadius > 0f)
		{
			map.glowGrid.DeregisterTerrain(c);
		}
		foundationGrid[num] = newTerr;
		if (newTerr.glowRadius > 0f)
		{
			map.glowGrid.RegisterTerrain(c);
		}
		DoTerrainChangedEffects(c, oldTerr, newTerr);
	}

	public void RemoveFoundation(IntVec3 c, bool doLeavings = true)
	{
		int num = map.cellIndices.CellToIndex(c);
		TerrainDef oldTerr = TerrainAt(num);
		TerrainDef oldFoundation = FoundationAt(num);
		if (doLeavings)
		{
			GenLeaving.DoLeavingsFor(foundationGrid[num], c, map);
		}
		if (foundationGrid[num].glowRadius > 0f)
		{
			map.glowGrid.DeregisterTerrain(c);
		}
		foundationGrid[num] = null;
		colorGrid[num] = null;
		if (topGrid[num].glowRadius > 0f)
		{
			map.glowGrid.RegisterTerrain(c);
		}
		DoTerrainChangedEffects(c, oldTerr, TerrainAt(num), oldFoundation);
	}

	public void RemoveGravshipTerrainUnsafe(IntVec3 cell, int index)
	{
		TerrainDef terrainDef = TerrainAt(cell);
		TerrainDef terrainDef2 = underGrid[index] ?? topGrid[index];
		if (terrainDef2 != null)
		{
			if (terrainDef.glowRadius > 0f)
			{
				map.glowGrid.DeregisterTerrain(cell);
			}
			tempGrid[index] = null;
			foundationGrid[index] = null;
			colorGrid[index] = null;
			underGrid[index] = null;
			topGrid[index] = terrainDef2;
			if (terrainDef2.glowRadius > 0f)
			{
				map.glowGrid.RegisterTerrain(cell);
			}
		}
	}

	public void SetTempTerrain(IntVec3 c, TerrainDef newTerr)
	{
		if (newTerr == null)
		{
			Log.Error($"Tried to set terrain at {c} to null.");
			return;
		}
		if (!newTerr.temporary)
		{
			Log.Error($"Tried to set temp terrain at {c} to a type which is not temporary ({newTerr.defName}).");
			return;
		}
		TerrainDef oldTerr = TerrainAt(c);
		if (Current.ProgramState == ProgramState.Playing)
		{
			map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor)?.Delete();
		}
		int num = map.cellIndices.CellToIndex(c);
		if (topGrid[num] != null && topGrid[num].glowRadius > 0f)
		{
			map.glowGrid.DeregisterTerrain(c);
		}
		TempTerrainProps tempTerrain = newTerr.tempTerrain;
		if (tempTerrain != null && tempTerrain.destroysFloors && underGrid[num] != null && !topGrid[num].natural)
		{
			topGrid[num] = underGrid[num];
			underGrid[num] = null;
			colorGrid[num] = null;
		}
		tempGrid[num] = newTerr;
		if (newTerr.glowRadius > 0f)
		{
			map.glowGrid.RegisterTerrain(c);
		}
		DoTerrainChangedEffects(c, oldTerr, newTerr);
	}

	public void RemoveTempTerrain(IntVec3 c, bool doLeavings = false, bool preventDestroyEffects = false)
	{
		int num = map.cellIndices.CellToIndex(c);
		if (doLeavings)
		{
			GenLeaving.DoLeavingsFor(tempGrid[num], c, map);
		}
		if (tempGrid[num] == null)
		{
			return;
		}
		TerrainDef terrainDef = tempGrid[num];
		if (terrainDef.glowRadius > 0f)
		{
			map.glowGrid.DeregisterTerrain(c);
		}
		tempGrid[num] = null;
		if (!preventDestroyEffects && terrainDef.tempTerrain?.terrainOnRemoved != null)
		{
			if (terrainDef.tempTerrain.terrainOnRemoved.temporary)
			{
				SetTempTerrain(c, terrainDef.tempTerrain.terrainOnRemoved);
			}
			else
			{
				SetTerrain(c, terrainDef.tempTerrain.terrainOnRemoved);
			}
		}
		else if (topGrid[num].glowRadius > 0f)
		{
			map.glowGrid.RegisterTerrain(c);
		}
		DoTerrainChangedEffects(c, terrainDef, TerrainAt(c));
	}

	public void SetTerrainColor(IntVec3 c, ColorDef color)
	{
		int num = map.cellIndices.CellToIndex(c);
		colorGrid[num] = color;
		DoTerrainChangedEffects(c, TerrainAt(c), TerrainAt(c));
	}

	public bool CanRemoveTopLayerAt(IntVec3 c)
	{
		int num = map.cellIndices.CellToIndex(c);
		TerrainDef obj = tempGrid[num];
		if (obj != null && obj.Removable)
		{
			return true;
		}
		if (topGrid[num].Removable)
		{
			return underGrid[num] != null;
		}
		return false;
	}

	public bool CanRemoveFoundationAt(IntVec3 c)
	{
		int num = map.cellIndices.CellToIndex(c);
		if (foundationGrid[num] != null)
		{
			return foundationGrid[num].Removable;
		}
		return false;
	}

	private void DoTerrainChangedEffects(IntVec3 c, TerrainDef oldTerr, TerrainDef newTerr, TerrainDef oldFoundation = null)
	{
		map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
		List<Thing> thingList = c.GetThingList(map);
		for (int num = thingList.Count - 1; num >= 0; num--)
		{
			if (thingList[num].def.category == ThingCategory.Plant)
			{
				if (map.fertilityGrid.FertilityAt(c) < thingList[num].def.plant.fertilityMin)
				{
					if (newTerr.IsFlood && !thingList[num].def.plant.destroyedByFlooding)
					{
						continue;
					}
					if (!thingList[num].def.plant.completelyIgnoreFertility)
					{
						thingList[num].Destroy();
						continue;
					}
				}
				if (thingList[num].def.plant.WildTerrainTags.Count > 0 && !thingList[num].def.plant.WildTerrainTags.Overlaps(newTerr.tags.OrElseEmptyEnumerable()))
				{
					thingList[num].Destroy();
					continue;
				}
				if (!thingList[num].def.plant.showInFrozenWater)
				{
					map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Things);
				}
			}
			if (thingList[num].def.category == ThingCategory.Filth && !FilthMaker.TerrainAcceptsFilth(newTerr, thingList[num].def))
			{
				thingList[num].Destroy();
			}
			else if ((thingList[num].def.IsBlueprint || thingList[num].def.IsFrame) && !GenConstruct.CanBuildOnTerrain(thingList[num].def.entityDefToBuild, thingList[num].Position, map, thingList[num].Rotation, thingList[num], ((IConstructible)thingList[num]).EntityToBuildStuff()))
			{
				thingList[num].Destroy(DestroyMode.Cancel);
			}
		}
		if (!newTerr.holdSnowOrSand)
		{
			map.snowGrid.SetDepth(c, 0f);
			map.sandGrid?.SetDepth(c, 0f);
		}
		map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor)?.Delete();
		map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor)?.Delete();
		map.designationManager.DesignationAt(c, DesignationDefOf.PaintFloor)?.Delete();
		map.designationManager.DesignationAt(c, DesignationDefOf.RemovePaintFloor)?.Delete();
		map.pathing.RecalculatePerceivedPathCostAt(c);
		drawerInt?.SetDirty();
		map.fertilityGrid.Drawer.SetDirty();
		if (ModsConfig.OdysseyActive && ((oldFoundation != null && oldFoundation.IsSubstructure) || newTerr.IsSubstructure))
		{
			map.substructureGrid?.MarkDirty();
		}
		map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c)?.District?.Notify_TerrainChanged();
		if (ModsConfig.OdysseyActive)
		{
			using (new ProfilerBlock("FishPopulationTracker.Notify_TerrainChanged"))
			{
				map.waterBodyTracker?.Notify_TerrainChanged(c, oldTerr, newTerr);
			}
		}
		map.events.Notify_TerrainChanged(c);
	}

	public void ExposeData()
	{
		ExposeTerrainGrid(topGrid, "topGrid", TerrainDefOf.Soil);
		ExposeTerrainGrid(underGrid, "underGrid", null);
		ExposeTerrainGrid(foundationGrid, "foundationGrid", null);
		ExposeTerrainGrid(tempGrid, "tempGrid", null);
		ExposeColorGrid();
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		foreach (IntVec3 allCell in map.AllCells)
		{
			int num = map.cellIndices.CellToIndex(allCell);
			if (TerrainAt(num).glowRadius > 0f)
			{
				map.glowGrid.RegisterTerrain(allCell);
			}
			if (topGrid[num].IsWater)
			{
				waterCells.Add(allCell);
			}
			if (topGrid[num].isFoundation)
			{
				foundationGrid[num] = topGrid[num];
				topGrid[num] = underGrid[num];
				underGrid[num] = null;
			}
		}
	}

	public void Notify_TerrainBurned(IntVec3 c)
	{
		TerrainDef terrain = c.GetTerrain(map);
		TerrainDef terrainDef = FoundationAt(c);
		if (terrainDef != null && terrainDef.Flammable())
		{
			Notify_FoundationDestroyed(c);
			return;
		}
		Notify_TerrainDestroyed(c);
		if (terrain.burnedDef != null)
		{
			SetTerrain(c, terrain.burnedDef);
		}
	}

	public void Notify_TerrainDestroyed(IntVec3 c)
	{
		if (CanRemoveTopLayerAt(c))
		{
			TerrainDef terrainDef = TerrainAt(c);
			RemoveTopLayer(c, doLeavings: false);
			if (terrainDef.destroyBuildingsOnDestroyed)
			{
				c.GetFirstBuilding(map)?.Kill();
			}
			if (terrainDef.destroyEffectWater != null && TerrainAt(c) != null && TerrainAt(c).IsWater)
			{
				Effecter effecter = terrainDef.destroyEffectWater.Spawn();
				effecter.Trigger(new TargetInfo(c, map), new TargetInfo(c, map));
				effecter.Cleanup();
			}
			else if (terrainDef.destroyEffect != null)
			{
				Effecter effecter2 = terrainDef.destroyEffect.Spawn();
				effecter2.Trigger(new TargetInfo(c, map), new TargetInfo(c, map));
				effecter2.Cleanup();
			}
			ThingUtility.CheckAutoRebuildTerrainOnDestroyed(terrainDef, c, map);
		}
	}

	public void Notify_FoundationDestroyed(IntVec3 c)
	{
		TerrainDef terrainDef = FoundationAt(c);
		if (terrainDef != null)
		{
			if (CanRemoveTopLayerAt(c))
			{
				Notify_TerrainDestroyed(c);
			}
			RemoveFoundation(c, doLeavings: false);
			if (terrainDef.destroyBuildingsOnDestroyed)
			{
				c.GetFirstBuilding(map)?.Kill();
			}
			if (terrainDef.destroyEffectWater != null && TerrainAt(c) != null && TerrainAt(c).IsWater)
			{
				Effecter effecter = terrainDef.destroyEffectWater.Spawn();
				effecter.Trigger(new TargetInfo(c, map), new TargetInfo(c, map));
				effecter.Cleanup();
			}
			else if (terrainDef.destroyEffect != null)
			{
				Effecter effecter2 = terrainDef.destroyEffect.Spawn();
				effecter2.Trigger(new TargetInfo(c, map), new TargetInfo(c, map));
				effecter2.Cleanup();
			}
			ThingUtility.CheckAutoRebuildTerrainOnDestroyed(terrainDef, c, map);
		}
	}

	private void ExposeTerrainGrid(TerrainDef[] grid, string label, TerrainDef fallbackTerrain)
	{
		Dictionary<ushort, TerrainDef> terrainDefsByShortHash = new Dictionary<ushort, TerrainDef>();
		foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
		{
			terrainDefsByShortHash.Add(allDef.shortHash, allDef);
		}
		MapExposeUtility.ExposeUshort(map, Read, Write, label);
		ushort Read(IntVec3 c)
		{
			return grid[map.cellIndices.CellToIndex(c)]?.shortHash ?? 0;
		}
		void Write(IntVec3 c, ushort val)
		{
			TerrainDef terrainDef = terrainDefsByShortHash.TryGetValue(val);
			if (terrainDef == null && val != 0)
			{
				TerrainDef terrainDef2 = BackCompatibility.BackCompatibleTerrainWithShortHash(val);
				if (terrainDef2 == null)
				{
					Log.Error($"Did not find terrain def with short hash {val} for cell {c}.");
					terrainDef2 = TerrainDefOf.Soil;
				}
				terrainDef = terrainDef2;
				terrainDefsByShortHash.Add(val, terrainDef2);
			}
			if (terrainDef == null && fallbackTerrain != null)
			{
				Log.ErrorOnce($"Replacing missing terrain with {fallbackTerrain}", Gen.HashCombine(8388383, fallbackTerrain.shortHash));
				terrainDef = fallbackTerrain;
			}
			grid[map.cellIndices.CellToIndex(c)] = terrainDef;
		}
	}

	private void ExposeColorGrid()
	{
		Dictionary<ushort, ColorDef> colorDefsByShortHash = new Dictionary<ushort, ColorDef>();
		foreach (ColorDef allDef in DefDatabase<ColorDef>.AllDefs)
		{
			colorDefsByShortHash.Add(allDef.shortHash, allDef);
		}
		MapExposeUtility.ExposeUshort(map, Read, Write, "colorGrid");
		ushort Read(IntVec3 c)
		{
			return colorGrid[map.cellIndices.CellToIndex(c)]?.shortHash ?? 0;
		}
		void Write(IntVec3 c, ushort val)
		{
			ColorDef colorDef = colorDefsByShortHash.TryGetValue(val);
			if (colorDef == null && val != 0)
			{
				Log.Error($"Did not find color def with short hash {val} for cell {c}.");
			}
			colorGrid[map.cellIndices.CellToIndex(c)] = colorDef;
		}
	}

	public string DebugStringAt(IntVec3 c)
	{
		if (c.InBounds(map))
		{
			TerrainDef terrain = c.GetTerrain(map);
			TerrainDef terrainDef = underGrid[map.cellIndices.CellToIndex(c)];
			return "top: " + ((terrain != null) ? terrain.defName : "null") + ", under=" + ((terrainDef != null) ? terrainDef.defName : "null");
		}
		return "out of bounds";
	}

	public void TerrainGridUpdate()
	{
		if (Find.PlaySettings.showTerrainAffordanceOverlay && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
		}
		Drawer.CellBoolDrawerUpdate();
	}

	private Color CellBoolDrawerColorInt()
	{
		return Color.white;
	}

	private bool CellBoolDrawerGetBoolInt(int index)
	{
		IntVec3 intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
		if (intVec.Filled(map) || intVec.Fogged(map))
		{
			return false;
		}
		TerrainAffordanceDef affordance;
		return TryGetAffordanceDefToDraw(intVec, out affordance);
	}

	private bool TryGetAffordanceDefToDraw(IntVec3 cell, out TerrainAffordanceDef affordance)
	{
		if (cell.GetAffordances(map).NullOrEmpty())
		{
			affordance = null;
			return true;
		}
		TerrainAffordanceDef terrainAffordanceDef = null;
		int num = int.MinValue;
		foreach (TerrainAffordanceDef affordance2 in cell.GetAffordances(map))
		{
			if (affordance2.visualizeOnAffordanceOverlay)
			{
				if (num < affordance2.order)
				{
					num = affordance2.order;
					terrainAffordanceDef = affordance2;
				}
			}
			else if (affordance2.blockAffordanceOverlay)
			{
				affordance = null;
				return false;
			}
		}
		affordance = terrainAffordanceDef;
		return true;
	}

	private Color CellBoolDrawerGetExtraColorInt(int index)
	{
		IntVec3 cell = CellIndicesUtility.IndexToCell(index, map.Size.x);
		TryGetAffordanceDefToDraw(cell, out var affordance);
		return affordance?.affordanceOverlayColor ?? NoAffordanceColor;
	}
}
