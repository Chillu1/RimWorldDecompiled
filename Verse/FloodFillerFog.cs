using System;
using System.Collections.Generic;

namespace Verse
{
	public static class FloodFillerFog
	{
		private static bool testMode = false;

		private static List<IntVec3> cellsToUnfog = new List<IntVec3>(1024);

		private const int MaxNumTestUnfog = 500;

		public static FloodUnfogResult FloodUnfog(IntVec3 root, Map map)
		{
			cellsToUnfog.Clear();
			FloodUnfogResult result = default(FloodUnfogResult);
			bool[] fogGridDirect = map.fogGrid.fogGrid;
			FogGrid fogGrid = map.fogGrid;
			List<IntVec3> newlyUnfoggedCells = new List<IntVec3>();
			int numUnfogged = 0;
			bool expanding = false;
			CellRect viewRect = CellRect.ViewRect(map);
			result.allOnScreen = true;
			Predicate<IntVec3> predicate = delegate(IntVec3 c)
			{
				if (!fogGridDirect[map.cellIndices.CellToIndex(c)])
				{
					return false;
				}
				Thing edifice = c.GetEdifice(map);
				if (edifice != null && edifice.def.MakeFog)
				{
					return false;
				}
				return (!testMode || expanding || numUnfogged <= 500) ? true : false;
			};
			Action<IntVec3> processor = delegate(IntVec3 c)
			{
				fogGrid.Unfog(c);
				newlyUnfoggedCells.Add(c);
				List<Thing> thingList = c.GetThingList(map);
				for (int l = 0; l < thingList.Count; l++)
				{
					Pawn pawn = thingList[l] as Pawn;
					if (pawn != null)
					{
						pawn.mindState.Active = true;
						if (pawn.def.race.IsMechanoid)
						{
							result.mechanoidFound = true;
						}
					}
				}
				if (!viewRect.Contains(c))
				{
					result.allOnScreen = false;
				}
				result.cellsUnfogged++;
				if (testMode)
				{
					numUnfogged++;
					map.debugDrawer.FlashCell(c, (float)numUnfogged / 200f, numUnfogged.ToStringCached());
				}
			};
			map.floodFiller.FloodFill(root, predicate, processor);
			expanding = true;
			for (int i = 0; i < newlyUnfoggedCells.Count; i++)
			{
				IntVec3 a = newlyUnfoggedCells[i];
				for (int j = 0; j < 8; j++)
				{
					IntVec3 intVec = a + GenAdj.AdjacentCells[j];
					if (intVec.InBounds(map) && fogGrid.IsFogged(intVec) && !predicate(intVec))
					{
						cellsToUnfog.Add(intVec);
					}
				}
			}
			for (int k = 0; k < cellsToUnfog.Count; k++)
			{
				fogGrid.Unfog(cellsToUnfog[k]);
				if (testMode)
				{
					map.debugDrawer.FlashCell(cellsToUnfog[k], 0.3f, "x");
				}
			}
			cellsToUnfog.Clear();
			return result;
		}

		public static void DebugFloodUnfog(IntVec3 root, Map map)
		{
			map.fogGrid.SetAllFogged();
			foreach (IntVec3 allCell in map.AllCells)
			{
				map.mapDrawer.MapMeshDirty(allCell, MapMeshFlag.FogOfWar);
			}
			testMode = true;
			FloodUnfog(root, map);
			testMode = false;
		}

		public static void DebugRefogMap(Map map)
		{
			map.fogGrid.SetAllFogged();
			foreach (IntVec3 allCell in map.AllCells)
			{
				map.mapDrawer.MapMeshDirty(allCell, MapMeshFlag.FogOfWar);
			}
			FloodUnfog(map.mapPawns.FreeColonistsSpawned.RandomElement().Position, map);
		}
	}
}
