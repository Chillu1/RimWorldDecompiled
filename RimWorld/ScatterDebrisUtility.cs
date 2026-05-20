using System;
using Verse;

namespace RimWorld;

public static class ScatterDebrisUtility
{
	public static bool CanPlaceThingAt(IntVec3 c, Rot4 rot, Map map, ThingDef thingDef)
	{
		if (!c.InBounds(map))
		{
			return false;
		}
		if (!GenConstruct.CanBuildOnTerrain(thingDef, c, map, rot))
		{
			return false;
		}
		foreach (IntVec3 item in GenAdj.OccupiedRect(c, rot, thingDef.size))
		{
			if (!item.InBounds(map) || item.Roofed(map) || item.GetEdifice(map) != null || !map.reachability.CanReachMapEdge(item, TraverseParms.For(TraverseMode.NoPassClosedDoors)))
			{
				return false;
			}
			if (item.GetThingList(map).Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static void ScatterFilthAroundThing(Thing thing, Map map, ThingDef filth, float scatterChance = 0.5f, int expandBy = 1, int maxFilth = int.MaxValue, Func<IntVec3, bool> cellValidator = null)
	{
		int num = 0;
		foreach (IntVec3 item in thing.OccupiedRect().ExpandedBy(expandBy).InRandomOrder())
		{
			if (item.InBounds(map) && Rand.Chance(scatterChance) && (cellValidator == null || cellValidator(item)) && FilthMaker.TryMakeFilth(item, map, filth))
			{
				num++;
			}
			if (num >= maxFilth)
			{
				break;
			}
		}
	}

	public static void ScatterFilthAroundCell(IntVec3 pos, IntVec2 area, Map map, ThingDef filth, float scatterChance = 0.5f, int maxFilth = int.MaxValue, Func<IntVec3, bool> cellValidator = null)
	{
		int num = 0;
		foreach (IntVec3 item in pos.RectAbout(area).InRandomOrder())
		{
			if (item.InBounds(map) && Rand.Chance(scatterChance) && (cellValidator == null || cellValidator(item)) && FilthMaker.TryMakeFilth(item, map, filth))
			{
				num++;
			}
			if (num >= maxFilth)
			{
				break;
			}
		}
	}

	public static void ScatterFilthAroundThing(Thing thing, Map map, ThingDef filth, IntRange count, int expandBy = 1, Func<IntVec3, bool> cellValidator = null)
	{
		int num = 0;
		int randomInRange = count.RandomInRange;
		foreach (IntVec3 item in thing.OccupiedRect().ExpandedBy(expandBy).InRandomOrder())
		{
			if (item.InBounds(map) && (cellValidator == null || cellValidator(item)) && FilthMaker.TryMakeFilth(item, map, filth))
			{
				num++;
			}
			if (num >= randomInRange)
			{
				break;
			}
		}
	}

	public static void ScatterAround(IntVec3 center, IntVec2 size, Rot4 rot, Sketch sketch, ThingDef scatterThing, float scatterChance = 0.5f, int expandBy = 1, int maxFilth = int.MaxValue, Func<IntVec3, bool> cellValidator = null)
	{
		int num = 0;
		foreach (IntVec3 item in GenAdj.OccupiedRect(center, rot, size).ExpandedBy(expandBy).InRandomOrder())
		{
			if (Rand.Chance(scatterChance) && (cellValidator == null || cellValidator(item)))
			{
				sketch.AddThing(scatterThing, item, Rot4.North);
				num++;
			}
			if (num >= maxFilth)
			{
				break;
			}
		}
	}
}
