using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
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

		public static bool TryPlaceThing(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null, Rot4 rot = default(Rot4))
		{
			Thing lastResultingThing;
			return TryPlaceThing(thing, center, map, mode, out lastResultingThing, placedAction, nearPlaceValidator, rot);
		}

		public static bool TryPlaceThing(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null, Rot4 rot = default(Rot4))
		{
			if (map == null)
			{
				Log.Error(string.Concat("Tried to place thing ", thing, " in a null map."));
				lastResultingThing = null;
				return false;
			}
			if (thing.def.category == ThingCategory.Filth)
			{
				mode = ThingPlaceMode.Direct;
			}
			switch (mode)
			{
			case ThingPlaceMode.Direct:
				return TryPlaceDirect(thing, center, rot, map, out lastResultingThing, placedAction);
			case ThingPlaceMode.Near:
			{
				lastResultingThing = null;
				int num = -1;
				do
				{
					num = thing.stackCount;
					if (!TryFindPlaceSpotNear(center, rot, map, thing, allowStacking: true, out var bestSpot, nearPlaceValidator))
					{
						return false;
					}
					if (TryPlaceDirect(thing, bestSpot, rot, map, out lastResultingThing, placedAction))
					{
						return true;
					}
				}
				while (thing.stackCount != num);
				Log.Error(string.Concat("Failed to place ", thing, " at ", center, " in mode ", mode, "."));
				lastResultingThing = null;
				return false;
			}
			default:
				throw new InvalidOperationException();
			}
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

		private static PlaceSpotQuality PlaceSpotQualityAt(IntVec3 c, Rot4 rot, Map map, Thing thing, IntVec3 center, bool allowStacking, Predicate<IntVec3> extraValidator = null)
		{
			if (!c.InBounds(map) || !c.Walkable(map))
			{
				return PlaceSpotQuality.Unusable;
			}
			if (!GenAdj.OccupiedRect(c, rot, thing.def.Size).InBounds(map))
			{
				return PlaceSpotQuality.Unusable;
			}
			if (extraValidator != null && !extraValidator(c))
			{
				return PlaceSpotQuality.Unusable;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing.def.saveCompressible && thing2.def.saveCompressible)
				{
					return PlaceSpotQuality.Unusable;
				}
				if (thing.def.category == ThingCategory.Item && thing2.def.category == ThingCategory.Item && (!thing2.CanStackWith(thing) || thing2.stackCount >= thing.def.stackLimit))
				{
					return PlaceSpotQuality.Unusable;
				}
			}
			if (thing is Building)
			{
				foreach (IntVec3 item in GenAdj.OccupiedRect(c, rot, thing.def.size))
				{
					Building edifice = item.GetEdifice(map);
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
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing3 = list[j];
					if (thing3.def.category == ThingCategory.Item && thing3.CanStackWith(thing) && thing3.stackCount < thing.def.stackLimit)
					{
						return PlaceSpotQuality.Perfect;
					}
				}
			}
			bool flag = (thing as Pawn)?.Downed ?? false;
			PlaceSpotQuality placeSpotQuality = PlaceSpotQuality.Perfect;
			for (int k = 0; k < list.Count; k++)
			{
				Thing thing4 = list[k];
				if (thing4.def.IsDoor)
				{
					return PlaceSpotQuality.Bad;
				}
				if (thing4 is Building_WorkTable)
				{
					return PlaceSpotQuality.Bad;
				}
				Pawn pawn = thing4 as Pawn;
				if (pawn != null)
				{
					if (pawn.Downed || flag)
					{
						return PlaceSpotQuality.Bad;
					}
					if ((int)placeSpotQuality > 3)
					{
						placeSpotQuality = PlaceSpotQuality.Okay;
					}
				}
				if (thing4.def.category == ThingCategory.Plant && thing4.def.selectable && (int)placeSpotQuality > 3)
				{
					placeSpotQuality = PlaceSpotQuality.Okay;
				}
			}
			return placeSpotQuality;
		}

		private static bool TryPlaceDirect(Thing thing, IntVec3 loc, Rot4 rot, Map map, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			Thing thing2 = thing;
			bool flag = false;
			if (thing.stackCount > thing.def.stackLimit)
			{
				thing = thing.SplitOff(thing.def.stackLimit);
				flag = true;
			}
			if (thing.def.stackLimit > 1)
			{
				List<Thing> thingList = loc.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing3 = thingList[i];
					if (thing3.CanStackWith(thing))
					{
						int stackCount = thing.stackCount;
						if (thing3.TryAbsorbStack(thing, respectStackLimit: true))
						{
							resultingThing = thing3;
							placedAction?.Invoke(thing3, stackCount);
							return !flag;
						}
						resultingThing = null;
						if (placedAction != null && stackCount != thing.stackCount)
						{
							placedAction(thing3, stackCount - thing.stackCount);
						}
						if (thing2 != thing)
						{
							thing2.TryAbsorbStack(thing, respectStackLimit: false);
						}
						return false;
					}
				}
			}
			resultingThing = GenSpawn.Spawn(thing, loc, map, rot);
			placedAction?.Invoke(thing, thing.stackCount);
			return !flag;
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
				if ((thing.def.category != ThingCategory.Plant || thing.def.passability != 0) && thing.def.category != ThingCategory.Filth && (haulThing == null || thing.def.category != ThingCategory.Item || !thing.CanStackWith(haulThing) || thing.def.stackLimit - thing.stackCount < haulThing.stackCount))
				{
					if (thing.def.EverHaulable)
					{
						return thing;
					}
					if (haulThing != null && GenSpawn.SpawningWipes(haulThing.def, thing.def))
					{
						return thing;
					}
					if (thing.def.passability != 0 && thing.def.surfaceType != SurfaceType.Item)
					{
						return thing;
					}
				}
			}
			return null;
		}
	}
}
