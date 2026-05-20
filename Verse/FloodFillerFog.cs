using System;
using System.Collections.Generic;
using RimWorld;
using Unity.Collections;

namespace Verse;

public static class FloodFillerFog
{
	private static bool testMode = false;

	private static List<IntVec3> cellsToUnfog = new List<IntVec3>(1024);

	private const int MaxNumTestUnfog = 500;

	public static FloodUnfogResult FloodUnfog(IntVec3 root, Map map)
	{
		cellsToUnfog.Clear();
		FloodUnfogResult result = default(FloodUnfogResult);
		NativeBitArray fogGridDirect = map.fogGrid.FogGrid_Unsafe;
		FogGrid fogGrid = map.fogGrid;
		List<IntVec3> newlyUnfoggedCells = new List<IntVec3>();
		int numUnfogged = 0;
		bool expanding = false;
		CellRect viewRect = CellRect.ViewRect(map);
		result.allOnScreen = true;
		map.floodFiller.FloodFill(root, (Predicate<IntVec3>)PassCheck, (Action<IntVec3>)Processor, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
		expanding = true;
		for (int i = 0; i < newlyUnfoggedCells.Count; i++)
		{
			IntVec3 intVec = newlyUnfoggedCells[i];
			for (int j = 0; j < 8; j++)
			{
				IntVec3 intVec2 = intVec + GenAdj.AdjacentCells[j];
				if (intVec2.InBounds(map) && fogGrid.IsFogged(intVec2) && !PassCheck(intVec2))
				{
					cellsToUnfog.Add(intVec2);
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
		bool PassCheck(IntVec3 c)
		{
			if (!fogGridDirect.IsSet(map.cellIndices.CellToIndex(c)))
			{
				return false;
			}
			Thing edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def.MakeFog)
			{
				return false;
			}
			if (testMode && !expanding && numUnfogged > 500)
			{
				return false;
			}
			return true;
		}
		void Processor(IntVec3 c)
		{
			fogGrid.Unfog(c);
			newlyUnfoggedCells.Add(c);
			List<Thing> thingList = c.GetThingList(map);
			for (int l = 0; l < thingList.Count; l++)
			{
				if (thingList[l] is Pawn pawn)
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
		}
	}

	public static void DebugFloodUnfog(IntVec3 root, Map map)
	{
		map.fogGrid.SetAllFogged();
		foreach (IntVec3 allCell in map.AllCells)
		{
			map.mapDrawer.MapMeshDirty(allCell, MapMeshFlagDefOf.FogOfWar);
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
			map.mapDrawer.MapMeshDirty(allCell, MapMeshFlagDefOf.FogOfWar);
		}
		FloodUnfog(map.mapPawns.FreeColonistsSpawned.RandomElement().Position, map);
	}
}
