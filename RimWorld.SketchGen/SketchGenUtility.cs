using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public static class SketchGenUtility
{
	private static HashSet<IntVec3> tmpProcessed = new HashSet<IntVec3>();

	public static bool IsStuffAllowed(ThingDef stuff, bool allowWood, Map useOnlyStonesAvailableOnMap, bool allowFlammableWalls, ThingDef stuffFor)
	{
		if (!allowWood && stuff == ThingDefOf.WoodLog)
		{
			return false;
		}
		if (!allowFlammableWalls && stuffFor == ThingDefOf.Wall && StatDefOf.Flammability.Worker.GetValueAbstract(stuffFor, stuff) > 0f)
		{
			return false;
		}
		if (useOnlyStonesAvailableOnMap != null && stuff.stuffProps.SourceNaturalRock != null && stuff.stuffProps.SourceNaturalRock.IsNonResourceNaturalRock && !Find.World.NaturalRockTypesIn(useOnlyStonesAvailableOnMap.Tile).Contains(stuff.stuffProps.SourceNaturalRock))
		{
			return false;
		}
		return true;
	}

	public static bool IsFloorAllowed(TerrainDef floor, bool allowWoodenFloor, bool allowConcrete, Map useOnlyStonesAvailableOnMap, bool onlyBuildableByPlayer, bool onlyStoneFloor)
	{
		if (!allowWoodenFloor && floor == TerrainDefOf.WoodPlankFloor)
		{
			return false;
		}
		if (!allowConcrete && (floor == TerrainDefOf.Concrete || floor == TerrainDefOf.AncientConcrete))
		{
			return false;
		}
		if (onlyStoneFloor)
		{
			List<ThingDefCountClass> list = floor.CostListAdjusted(null);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].thingDef.stuffProps == null || !list[i].thingDef.stuffProps.categories.Contains(StuffCategoryDefOf.Stony))
				{
					return false;
				}
			}
		}
		if (useOnlyStonesAvailableOnMap != null)
		{
			bool flag = false;
			bool flag2 = true;
			List<ThingDefCountClass> list2 = floor.CostListAdjusted(null);
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].thingDef.stuffProps != null && list2[j].thingDef.stuffProps.SourceNaturalRock != null && list2[j].thingDef.stuffProps.SourceNaturalRock.IsNonResourceNaturalRock)
				{
					flag = true;
					flag2 = flag2 && Find.World.NaturalRockTypesIn(useOnlyStonesAvailableOnMap.Tile).Contains(list2[j].thingDef.stuffProps.SourceNaturalRock);
				}
			}
			if (flag && !flag2)
			{
				return false;
			}
		}
		if (onlyBuildableByPlayer && !PlayerCanBuildNow(floor))
		{
			return false;
		}
		return true;
	}

	public static CellRect FindBiggestRectAt(IntVec3 c, CellRect outerRect, Sketch sketch, HashSet<IntVec3> processed, Predicate<IntVec3> canTraverse)
	{
		if (processed.Contains(c) || !canTraverse(c))
		{
			return CellRect.Empty;
		}
		CellRect result = CellRect.SingleCell(c);
		bool flag;
		do
		{
			flag = false;
			if (result.maxX < outerRect.maxX)
			{
				bool flag2 = false;
				foreach (IntVec3 edgeCell in result.GetEdgeCells(Rot4.East))
				{
					IntVec3 current = edgeCell;
					current.x++;
					if (processed.Contains(current) || !canTraverse(current))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					result.maxX++;
					flag = true;
				}
			}
			if (result.minX > outerRect.minX)
			{
				bool flag3 = false;
				foreach (IntVec3 edgeCell2 in result.GetEdgeCells(Rot4.West))
				{
					IntVec3 current2 = edgeCell2;
					current2.x--;
					if (processed.Contains(current2) || !canTraverse(current2))
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					result.minX--;
					flag = true;
				}
			}
			if (result.maxZ < outerRect.maxZ)
			{
				bool flag4 = false;
				foreach (IntVec3 edgeCell3 in result.GetEdgeCells(Rot4.North))
				{
					IntVec3 current3 = edgeCell3;
					current3.z++;
					if (processed.Contains(current3) || !canTraverse(current3))
					{
						flag4 = true;
						break;
					}
				}
				if (!flag4)
				{
					result.maxZ++;
					flag = true;
				}
			}
			if (result.minZ <= outerRect.minZ)
			{
				continue;
			}
			bool flag5 = false;
			foreach (IntVec3 edgeCell4 in result.GetEdgeCells(Rot4.South))
			{
				IntVec3 current4 = edgeCell4;
				current4.z--;
				if (processed.Contains(current4) || !canTraverse(current4))
				{
					flag5 = true;
					break;
				}
			}
			if (!flag5)
			{
				result.minZ--;
				flag = true;
			}
		}
		while (flag);
		foreach (IntVec3 item in result)
		{
			processed.Add(item);
		}
		return result;
	}

	public static CellRect FindBiggestRect(Sketch sketch, Predicate<IntVec3> canTraverse, IEnumerable<IntVec3> cells = null, int tries = 3)
	{
		try
		{
			CellRect result = CellRect.Empty;
			for (int i = 0; i < tries; i++)
			{
				tmpProcessed.Clear();
				foreach (IntVec3 item in cells ?? sketch.OccupiedRect.InRandomOrder())
				{
					CellRect cellRect = FindBiggestRectAt(item, sketch.OccupiedRect, sketch, tmpProcessed, canTraverse);
					if (cellRect.Area > result.Area)
					{
						result = cellRect;
					}
				}
			}
			return result;
		}
		finally
		{
			tmpProcessed.Clear();
		}
	}

	public static bool PlayerCanBuildNow(BuildableDef buildable)
	{
		if (buildable.BuildableByPlayer)
		{
			return buildable.IsResearchFinished;
		}
		return false;
	}

	public static SketchThing GetDoor(this Sketch sketch, IntVec3 position)
	{
		foreach (SketchThing item in sketch.ThingsAt(position))
		{
			if (item.def.IsDoor)
			{
				return item;
			}
		}
		return null;
	}
}
