using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class DoorUtility
{
	public static IEnumerable<IntVec3> WallRequirementCells(ThingDef def, IntVec3 pos, Rot4 rot)
	{
		if (!(def.size == IntVec2.One))
		{
			CellRect rect = GenAdj.OccupiedRect(IntVec3.Zero, def.defaultPlacingRot, def.size);
			int max = (def.defaultPlacingRot.IsHorizontal ? rect.Width : rect.Height);
			for (int i = 0; i < max; i++)
			{
				yield return pos + new IntVec3(rect.minX - 1, 0, rect.minZ + i).RotatedBy(rot);
				yield return pos + new IntVec3(rect.maxX + 1, 0, rect.minZ + i).RotatedBy(rot);
			}
		}
	}

	public static bool EncapsulatingWallAt(IntVec3 cell, Map map, bool includeUnbuilt = false)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		if (includeUnbuilt)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Blueprint blueprint && blueprint.def.entityDefToBuild is ThingDef { Fillage: FillCategory.Full, IsDoor: false })
				{
					return true;
				}
				if (thingList[i] is Frame frame && frame.BuildDef.Fillage == FillCategory.Full && !frame.BuildDef.IsDoor)
				{
					return true;
				}
			}
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice == null || edifice.def.Fillage != FillCategory.Full || edifice.def.IsDoor)
		{
			return false;
		}
		return true;
	}

	public static Rot4 DoorRotationAt(IntVec3 loc, Map map, bool preferFences)
	{
		int num = 0;
		int num2 = 0 + AlignQualityAgainst(loc, IntVec3.East, map, preferFences) + AlignQualityAgainst(loc, IntVec3.West, map, preferFences);
		num += AlignQualityAgainst(loc, IntVec3.North, map, preferFences);
		num += AlignQualityAgainst(loc, IntVec3.South, map, preferFences);
		if (num2 >= num)
		{
			return Rot4.North;
		}
		return Rot4.East;
	}

	private static int AlignQualityAgainst(IntVec3 c, IntVec3 offset, Map map, bool preferFences)
	{
		IntVec3 c2 = c + offset;
		if (!c2.InBounds(map))
		{
			return 0;
		}
		if (!c2.WalkableByNormal(map))
		{
			return 9;
		}
		List<Thing> thingList = c2.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
			{
				if ((c - offset).GetDoor(map) == null)
				{
					return 1;
				}
				return 5;
			}
			if (thing.def.IsFence)
			{
				if (!preferFences)
				{
					return 1;
				}
				return 10;
			}
			Thing thing2 = thing as Blueprint;
			if (thing2 == null)
			{
				continue;
			}
			if (thing2.def.entityDefToBuild.passability == Traversability.Impassable)
			{
				return 9;
			}
			if (thing2.def.entityDefToBuild is ThingDef { IsFence: not false })
			{
				if (!preferFences)
				{
					return 1;
				}
				return 10;
			}
			if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
			{
				return 1;
			}
		}
		return 0;
	}
}
