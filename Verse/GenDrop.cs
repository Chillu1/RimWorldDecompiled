using System;
using Verse.Sound;

namespace Verse
{
	public static class GenDrop
	{
		public static bool TryDropSpawn(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
		{
			if (map == null)
			{
				Log.Error("Dropped " + thing + " in a null map.");
				resultingThing = null;
				return false;
			}
			if (!dropCell.InBounds(map))
			{
				Log.Error("Dropped " + thing + " out of bounds at " + dropCell);
				resultingThing = null;
				return false;
			}
			if (thing.def.destroyOnDrop)
			{
				thing.Destroy();
				resultingThing = null;
				return true;
			}
			if (thing.def.soundDrop != null)
			{
				thing.def.soundDrop.PlayOneShot(new TargetInfo(dropCell, map));
			}
			return GenPlace.TryPlaceThing(thing, dropCell, map, mode, out resultingThing, placedAction, nearPlaceValidator);
		}
	}
}
