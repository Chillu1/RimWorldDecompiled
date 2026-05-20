using System;
using Verse.Sound;

namespace Verse;

public static class GenDrop
{
	public static bool TryDropSpawn(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null, bool playDropSound = true)
	{
		if (map == null)
		{
			Log.Error("Dropped " + thing?.ToString() + " in a null map.");
			resultingThing = null;
			return false;
		}
		if (!dropCell.InBounds(map))
		{
			string obj = thing?.ToString();
			IntVec3 intVec = dropCell;
			Log.Error("Dropped " + obj + " out of bounds at " + intVec.ToString());
			resultingThing = null;
			return false;
		}
		if (thing.def.destroyOnDrop)
		{
			thing.Destroy();
			resultingThing = null;
			return true;
		}
		if (GenPlace.TryPlaceThing(thing, dropCell, map, mode, out resultingThing, placedAction, nearPlaceValidator))
		{
			if (playDropSound && thing.def.soundDrop != null)
			{
				thing.def.soundDrop.PlayOneShot(new TargetInfo(dropCell, map));
			}
			return true;
		}
		return false;
	}
}
