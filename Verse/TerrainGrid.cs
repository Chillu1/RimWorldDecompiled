using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public sealed class TerrainGrid : IExposable
	{
		private Map map;

		public TerrainDef[] topGrid;

		private TerrainDef[] underGrid;

		private CellBoolDrawer drawerInt;

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

		public TerrainGrid(Map map)
		{
			this.map = map;
			ResetGrids();
		}

		public void ResetGrids()
		{
			topGrid = new TerrainDef[map.cellIndices.NumGridCells];
			underGrid = new TerrainDef[map.cellIndices.NumGridCells];
		}

		public TerrainDef TerrainAt(int ind)
		{
			return topGrid[ind];
		}

		public TerrainDef TerrainAt(IntVec3 c)
		{
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

		public void SetTerrain(IntVec3 c, TerrainDef newTerr)
		{
			if (newTerr == null)
			{
				Log.Error(string.Concat("Tried to set terrain at ", c, " to null."));
				return;
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor)?.Delete();
			}
			int num = map.cellIndices.CellToIndex(c);
			if (newTerr.layerable)
			{
				if (underGrid[num] == null)
				{
					if (topGrid[num].passability != Traversability.Impassable)
					{
						underGrid[num] = topGrid[num];
					}
					else
					{
						underGrid[num] = TerrainDefOf.Sand;
					}
				}
			}
			else
			{
				underGrid[num] = null;
			}
			topGrid[num] = newTerr;
			DoTerrainChangedEffects(c);
		}

		public void SetUnderTerrain(IntVec3 c, TerrainDef newTerr)
		{
			if (!c.InBounds(map))
			{
				Log.Error("Tried to set terrain out of bounds at " + c);
				return;
			}
			int num = map.cellIndices.CellToIndex(c);
			underGrid[num] = newTerr;
		}

		public void RemoveTopLayer(IntVec3 c, bool doLeavings = true)
		{
			int num = map.cellIndices.CellToIndex(c);
			if (doLeavings)
			{
				GenLeaving.DoLeavingsFor(topGrid[num], c, map);
			}
			if (underGrid[num] != null)
			{
				topGrid[num] = underGrid[num];
				underGrid[num] = null;
				DoTerrainChangedEffects(c);
			}
		}

		public bool CanRemoveTopLayerAt(IntVec3 c)
		{
			int num = map.cellIndices.CellToIndex(c);
			if (topGrid[num].Removable)
			{
				return underGrid[num] != null;
			}
			return false;
		}

		private void DoTerrainChangedEffects(IntVec3 c)
		{
			map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
			List<Thing> thingList = c.GetThingList(map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				if (thingList[num].def.category == ThingCategory.Plant && map.fertilityGrid.FertilityAt(c) < thingList[num].def.plant.fertilityMin)
				{
					thingList[num].Destroy();
				}
				else if (thingList[num].def.category == ThingCategory.Filth && !FilthMaker.TerrainAcceptsFilth(TerrainAt(c), thingList[num].def))
				{
					thingList[num].Destroy();
				}
				else if ((thingList[num].def.IsBlueprint || thingList[num].def.IsFrame) && !GenConstruct.CanBuildOnTerrain(thingList[num].def.entityDefToBuild, thingList[num].Position, map, thingList[num].Rotation, null, ((IConstructible)thingList[num]).EntityToBuildStuff()))
				{
					thingList[num].Destroy(DestroyMode.Cancel);
				}
			}
			map.pathGrid.RecalculatePerceivedPathCostAt(c);
			if (drawerInt != null)
			{
				drawerInt.SetDirty();
			}
			map.fertilityGrid.Drawer.SetDirty();
			Region regionAt_NoRebuild_InvalidAllowed = map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c);
			if (regionAt_NoRebuild_InvalidAllowed != null && regionAt_NoRebuild_InvalidAllowed.Room != null)
			{
				regionAt_NoRebuild_InvalidAllowed.Room.Notify_TerrainChanged();
			}
		}

		public void ExposeData()
		{
			ExposeTerrainGrid(topGrid, "topGrid");
			ExposeTerrainGrid(underGrid, "underGrid");
		}

		public void Notify_TerrainBurned(IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain(map);
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
			}
		}

		private void ExposeTerrainGrid(TerrainDef[] grid, string label)
		{
			Dictionary<ushort, TerrainDef> terrainDefsByShortHash = new Dictionary<ushort, TerrainDef>();
			foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
			{
				terrainDefsByShortHash.Add(allDef.shortHash, allDef);
			}
			Func<IntVec3, ushort> shortReader = (IntVec3 c) => grid[map.cellIndices.CellToIndex(c)]?.shortHash ?? 0;
			Action<IntVec3, ushort> shortWriter = delegate(IntVec3 c, ushort val)
			{
				TerrainDef terrainDef = terrainDefsByShortHash.TryGetValue(val);
				if (terrainDef == null && val != 0)
				{
					TerrainDef terrainDef2 = BackCompatibility.BackCompatibleTerrainWithShortHash(val);
					if (terrainDef2 == null)
					{
						Log.Error(string.Concat("Did not find terrain def with short hash ", val, " for cell ", c, "."));
						terrainDef2 = TerrainDefOf.Soil;
					}
					terrainDef = terrainDef2;
					terrainDefsByShortHash.Add(val, terrainDef2);
				}
				grid[map.cellIndices.CellToIndex(c)] = terrainDef;
			};
			MapExposeUtility.ExposeUshort(map, shortReader, shortWriter, label);
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
			if (Find.PlaySettings.showTerrainAffordanceOverlay)
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
			IntVec3 c = CellIndicesUtility.IndexToCell(index, map.Size.x);
			if (c.Filled(map) || c.Fogged(map))
			{
				return false;
			}
			TerrainAffordanceDef affordance;
			return TryGetAffordanceDefToDraw(TerrainAt(index), out affordance);
		}

		private bool TryGetAffordanceDefToDraw(TerrainDef terrainDef, out TerrainAffordanceDef affordance)
		{
			if (terrainDef == null || terrainDef.affordances.NullOrEmpty())
			{
				affordance = null;
				return true;
			}
			TerrainAffordanceDef terrainAffordanceDef = null;
			int num = int.MinValue;
			foreach (TerrainAffordanceDef affordance2 in terrainDef.affordances)
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
			TryGetAffordanceDefToDraw(TerrainAt(index), out TerrainAffordanceDef affordance);
			return affordance?.affordanceOverlayColor ?? NoAffordanceColor;
		}
	}
}
