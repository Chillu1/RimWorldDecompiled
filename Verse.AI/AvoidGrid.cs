using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public class AvoidGrid
	{
		public Map map;

		private ByteGrid grid;

		private bool gridDirty = true;

		public ByteGrid Grid
		{
			get
			{
				if (gridDirty)
				{
					Regenerate();
				}
				return grid;
			}
		}

		public AvoidGrid(Map map)
		{
			this.map = map;
			grid = new ByteGrid(map);
		}

		public void Regenerate()
		{
			gridDirty = false;
			grid.Clear(0);
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def.building.ai_combatDangerous)
				{
					Building_TurretGun building_TurretGun = allBuildingsColonist[i] as Building_TurretGun;
					if (building_TurretGun != null)
					{
						PrintAvoidGridAroundTurret(building_TurretGun);
					}
				}
			}
			ExpandAvoidGridIntoEdifices();
		}

		public void Notify_BuildingSpawned(Building building)
		{
			if (building.def.building.ai_combatDangerous || !building.CanBeSeenOver())
			{
				gridDirty = true;
			}
		}

		public void Notify_BuildingDespawned(Building building)
		{
			if (building.def.building.ai_combatDangerous || !building.CanBeSeenOver())
			{
				gridDirty = true;
			}
		}

		public void DebugDrawOnMap()
		{
			if (DebugViewSettings.drawAvoidGrid && Find.CurrentMap == map)
			{
				Grid.DebugDraw();
			}
		}

		private void PrintAvoidGridAroundTurret(Building_TurretGun tur)
		{
			float range = tur.GunCompEq.PrimaryVerb.verbProps.range;
			float num = tur.GunCompEq.PrimaryVerb.verbProps.EffectiveMinRange(allowAdjacentShot: true);
			int num2 = GenRadial.NumCellsInRadius(range + 4f);
			for (int i = ((!(num < 1f)) ? GenRadial.NumCellsInRadius(num) : 0); i < num2; i++)
			{
				IntVec3 intVec = tur.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(tur.Map) && intVec.Walkable(tur.Map) && GenSight.LineOfSight(intVec, tur.Position, tur.Map, skipFirstCell: true))
				{
					IncrementAvoidGrid(intVec, 45);
				}
			}
		}

		private void IncrementAvoidGrid(IntVec3 c, int num)
		{
			byte b = grid[c];
			b = (byte)Mathf.Min(255, b + num);
			grid[c] = b;
		}

		private void ExpandAvoidGridIntoEdifices()
		{
			int numGridCells = map.cellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				if (grid[i] == 0 || map.edificeGrid[i] != null)
				{
					continue;
				}
				for (int j = 0; j < 8; j++)
				{
					IntVec3 c = map.cellIndices.IndexToCell(i) + GenAdj.AdjacentCells[j];
					if (c.InBounds(map) && c.GetEdifice(map) != null)
					{
						grid[c] = (byte)Mathf.Min(255, Mathf.Max(grid[c], grid[i]));
					}
				}
			}
		}
	}
}
