using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse;

public static class GenPlace
{
	private enum PlaceSpotQuality : byte
	{
		Unusable,
		Awful,
		Bad,
		Okay,
		Perfect
	}

	private static readonly int PlaceNearMaxRadialCells = GenRadial.NumCellsInRadius(12.9f);

	private static readonly int PlaceNearMiddleRadialCells = GenRadial.NumCellsInRadius(3f);

	private static List<Thing> thingList = new List<Thing>();

	private static List<Thing> cellThings = new List<Thing>(8);

	public static bool TryPlaceThing(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, Action<Thing, int> placedAction = null, Predicate<IntVec3> extraValidator = null, Rot4? rot = null, int squareRadius = 1)
	{
		Thing lastResultingThing;
		return TryPlaceThing(thing, center, map, mode, out lastResultingThing, placedAction, extraValidator, rot);
	}

	public static bool TryPlaceThing(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> extraValidator = null, Rot4? rot = null, int squareRadius = 1)
	{
		Rot4 valueOrDefault = rot.GetValueOrDefault();
		if (!rot.HasValue)
		{
			valueOrDefault = thing.def.defaultPlacingRot;
			rot = valueOrDefault;
		}
		lastResultingThing = null;
		if (map == null)
		{
			Log.Error("Tried to place thing " + thing?.ToString() + " in a null map.");
			lastResultingThing = null;
			return false;
		}
		if (thing.def.category == ThingCategory.Filth)
		{
			mode = ThingPlaceMode.Direct;
		}
		if (mode == ThingPlaceMode.Direct)
		{
			return TryPlaceDirect(thing, center, rot.Value, map, out lastResultingThing, placedAction);
		}
		if (mode == ThingPlaceMode.Near)
		{
			int stackCount;
			do
			{
				stackCount = thing.stackCount;
				if (!TryFindPlaceSpotNear(center, rot.Value, map, thing, allowStacking: true, out var bestSpot, extraValidator))
				{
					return false;
				}
				if (TryPlaceDirect(thing, bestSpot, rot.Value, map, out lastResultingThing, placedAction))
				{
					return true;
				}
			}
			while (thing.stackCount != stackCount);
			string[] obj = new string[7]
			{
				"Failed to place ",
				thing?.ToString(),
				" at ",
				null,
				null,
				null,
				null
			};
			IntVec3 intVec = center;
			obj[3] = intVec.ToString();
			obj[4] = " in mode ";
			obj[5] = mode.ToString();
			obj[6] = ".";
			Log.Error(string.Concat(obj));
			lastResultingThing = null;
			return false;
		}
		if (mode == ThingPlaceMode.Radius)
		{
			int stackCount2;
			do
			{
				stackCount2 = thing.stackCount;
				if (!TryFindPlaceSpotInRadius(center, rot.Value, map, thing, squareRadius, allowStacking: true, out var bestSpot2, 100, extraValidator))
				{
					return false;
				}
				if (TryPlaceDirect(thing, bestSpot2, rot.Value, map, out lastResultingThing, placedAction))
				{
					return true;
				}
			}
			while (thing.stackCount != stackCount2);
			string[] obj2 = new string[7]
			{
				"Failed to place ",
				thing?.ToString(),
				" at ",
				null,
				null,
				null,
				null
			};
			IntVec3 intVec = center;
			obj2[3] = intVec.ToString();
			obj2[4] = " in mode ";
			obj2[5] = mode.ToString();
			obj2[6] = ".";
			Log.Error(string.Concat(obj2));
			lastResultingThing = null;
			return false;
		}
		throw new InvalidOperationException();
	}

	private static bool TryFindPlaceSpotNear(IntVec3 center, Rot4 rot, Map map, Thing thing, bool allowStacking, out IntVec3 bestSpot, Predicate<IntVec3> extraValidator = null)
	{
		PlaceSpotQuality placeSpotQuality = PlaceSpotQuality.Unusable;
		bestSpot = center;
		for (int i = 0; i < 9; i++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[i];
			PlaceSpotQuality placeSpotQuality2 = PlaceSpotQualityAt(intVec, rot, map, thing, center, allowStacking, extraValidator);
			if ((int)placeSpotQuality2 > (int)placeSpotQuality)
			{
				bestSpot = intVec;
				placeSpotQuality = placeSpotQuality2;
			}
			if (placeSpotQuality == PlaceSpotQuality.Perfect)
			{
				break;
			}
		}
		if ((int)placeSpotQuality >= 3)
		{
			return true;
		}
		for (int j = 0; j < PlaceNearMiddleRadialCells; j++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[j];
			PlaceSpotQuality placeSpotQuality2 = PlaceSpotQualityAt(intVec, rot, map, thing, center, allowStacking, extraValidator);
			if ((int)placeSpotQuality2 > (int)placeSpotQuality)
			{
				bestSpot = intVec;
				placeSpotQuality = placeSpotQuality2;
			}
			if (placeSpotQuality == PlaceSpotQuality.Perfect)
			{
				break;
			}
		}
		if ((int)placeSpotQuality >= 3)
		{
			return true;
		}
		for (int k = 0; k < PlaceNearMaxRadialCells; k++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[k];
			PlaceSpotQuality placeSpotQuality2 = PlaceSpotQualityAt(intVec, rot, map, thing, center, allowStacking, extraValidator);
			if ((int)placeSpotQuality2 > (int)placeSpotQuality)
			{
				bestSpot = intVec;
				placeSpotQuality = placeSpotQuality2;
			}
			if (placeSpotQuality == PlaceSpotQuality.Perfect)
			{
				break;
			}
		}
		if ((int)placeSpotQuality > 0)
		{
			return true;
		}
		bestSpot = center;
		return false;
	}

	private static bool TryFindPlaceSpotInRadius(IntVec3 center, Rot4 rot, Map map, Thing thing, int radius, bool allowStacking, out IntVec3 bestSpot, int attempts = 100, Predicate<IntVec3> extraValidator = null)
	{
		PlaceSpotQuality placeSpotQuality = PlaceSpotQuality.Unusable;
		bestSpot = center;
		while (attempts-- > 0)
		{
			if (CellFinder.TryRandomClosewalkCellNear(center, map, radius, out var result))
			{
				PlaceSpotQuality placeSpotQuality2 = PlaceSpotQualityAt(result, rot, map, thing, center, allowStacking, extraValidator);
				if ((int)placeSpotQuality2 > (int)placeSpotQuality)
				{
					bestSpot = result;
					placeSpotQuality = placeSpotQuality2;
				}
				if (placeSpotQuality == PlaceSpotQuality.Perfect)
				{
					break;
				}
			}
		}
		return (int)placeSpotQuality > 0;
	}

	private static PlaceSpotQuality PlaceSpotQualityAt(IntVec3 c, Rot4 rot, Map map, Thing thing, IntVec3 center, bool allowStacking, Predicate<IntVec3> extraValidator = null)
	{
		if (!GenSpawn.CanSpawnAt(thing.def, c, map, rot))
		{
			return PlaceSpotQuality.Unusable;
		}
		if (extraValidator != null && !extraValidator(c))
		{
			return PlaceSpotQuality.Unusable;
		}
		thingList.Clear();
		foreach (IntVec3 item in GenAdj.OccupiedRect(c, rot, thing.def.Size))
		{
			thingList.AddRange(item.GetThingList(map));
		}
		bool flag = false;
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing2 = thingList[i];
			if (thing.def.saveCompressible && thing2.def.saveCompressible)
			{
				return PlaceSpotQuality.Unusable;
			}
			if (thing.def.category == ThingCategory.Item && thing2.def.category == ThingCategory.Item && allowStacking && thing2.stackCount < thing2.def.stackLimit && thing2.CanStackWith(thing))
			{
				flag = true;
			}
		}
		if (thing.def.category == ThingCategory.Item && !flag && c.GetItemCount(map) >= c.GetMaxItemsAllowedInCell(map))
		{
			return PlaceSpotQuality.Unusable;
		}
		if (c.GetEdifice(map) is IHaulDestination haulDestination && !haulDestination.Accepts(thing))
		{
			return PlaceSpotQuality.Unusable;
		}
		if (thing is Building)
		{
			foreach (IntVec3 item2 in GenAdj.OccupiedRect(c, rot, thing.def.size))
			{
				Building edifice = item2.GetEdifice(map);
				if (edifice != null && GenSpawn.SpawningWipes(thing.def, edifice.def))
				{
					return PlaceSpotQuality.Awful;
				}
			}
		}
		if (c.GetRoom(map) != center.GetRoom(map))
		{
			if (!map.reachability.CanReach(center, c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
			{
				return PlaceSpotQuality.Awful;
			}
			return PlaceSpotQuality.Bad;
		}
		if (allowStacking)
		{
			for (int j = 0; j < thingList.Count; j++)
			{
				Thing thing3 = thingList[j];
				if (thing3.def.category == ThingCategory.Item && thing3.CanStackWith(thing) && thing3.stackCount < thing3.def.stackLimit)
				{
					return PlaceSpotQuality.Perfect;
				}
			}
		}
		bool flag2 = thing is Pawn pawn && pawn.Downed;
		PlaceSpotQuality placeSpotQuality = PlaceSpotQuality.Perfect;
		for (int k = 0; k < thingList.Count; k++)
		{
			Thing thing4 = thingList[k];
			if (thing4.def.Fillage == FillCategory.Full)
			{
				return PlaceSpotQuality.Bad;
			}
			if (thing4.def.preventDroppingThingsOn)
			{
				return PlaceSpotQuality.Bad;
			}
			if (thing4.def.IsDoor)
			{
				return PlaceSpotQuality.Bad;
			}
			if (thing4 is Building_WorkTable)
			{
				return PlaceSpotQuality.Bad;
			}
			if (thing4 is Pawn pawn2 && (pawn2.Downed || flag2))
			{
				return PlaceSpotQuality.Bad;
			}
			if (thing4.def.category == ThingCategory.Plant && thing4.def.selectable && (int)placeSpotQuality > 3)
			{
				placeSpotQuality = PlaceSpotQuality.Okay;
			}
		}
		return placeSpotQuality;
	}

	private static bool SplitAndSpawnOneStackOnCell(Thing thing, IntVec3 loc, Rot4 rot, Map map, out Thing resultingThing, Action<Thing, int> placedAction)
	{
		Thing thing2 = ((thing.stackCount <= thing.def.stackLimit) ? thing : thing.SplitOff(thing.def.stackLimit));
		resultingThing = GenSpawn.Spawn(thing2, loc, map, rot);
		placedAction?.Invoke(thing2, thing2.stackCount);
		return thing2 == thing;
	}

	private static bool TryPlaceDirect(Thing thing, IntVec3 loc, Rot4 rot, Map map, out Thing resultingThing, Action<Thing, int> placedAction = null)
	{
		resultingThing = null;
		cellThings.Clear();
		cellThings.AddRange(loc.GetThingList(map));
		cellThings.Sort((Thing lhs, Thing rhs) => rhs.stackCount.CompareTo(lhs.stackCount));
		if (thing.def.stackLimit > 1)
		{
			for (int num = 0; num < cellThings.Count; num++)
			{
				Thing thing2 = cellThings[num];
				if (thing2.CanStackWith(thing))
				{
					int stackCount = thing.stackCount;
					if (thing2.TryAbsorbStack(thing, respectStackLimit: true))
					{
						resultingThing = thing2;
						placedAction?.Invoke(thing2, stackCount);
						return true;
					}
					if (placedAction != null && stackCount != thing.stackCount)
					{
						placedAction(thing2, stackCount - thing.stackCount);
					}
				}
			}
		}
		int num3;
		if (thing.def.category == ThingCategory.Item)
		{
			int num2 = cellThings.Count((Thing cellThing) => cellThing.def.category == ThingCategory.Item);
			num3 = loc.GetMaxItemsAllowedInCell(map) - num2;
		}
		else
		{
			num3 = thing.stackCount + 1;
		}
		if (num3 <= 0 && thing.def.stackLimit <= 1)
		{
			num3 = 1;
		}
		for (int num4 = 0; num4 < num3; num4++)
		{
			if (SplitAndSpawnOneStackOnCell(thing, loc, rot, map, out resultingThing, placedAction))
			{
				return true;
			}
		}
		return false;
	}

	public static Thing HaulPlaceBlockerIn(Thing haulThing, IntVec3 c, Map map, bool checkBlueprintsAndFrames)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (checkBlueprintsAndFrames && (thing.def.IsBlueprint || thing.def.IsFrame))
			{
				return thing;
			}
			if ((thing.def.category != ThingCategory.Plant || thing.def.passability != Traversability.Standable) && thing.def.category != ThingCategory.Filth && (haulThing == null || thing.def.category != ThingCategory.Item || !thing.CanStackWith(haulThing) || thing.def.stackLimit - thing.stackCount < haulThing.stackCount))
			{
				if (thing.def.EverHaulable)
				{
					return thing;
				}
				if (haulThing != null && GenSpawn.SpawningWipes(haulThing.def, thing.def))
				{
					return thing;
				}
				if (thing.def.passability != Traversability.Standable && thing.def.surfaceType != SurfaceType.Item)
				{
					return thing;
				}
			}
		}
		return null;
	}
}
