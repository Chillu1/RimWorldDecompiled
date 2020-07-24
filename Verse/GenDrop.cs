using System;
using Verse.Sound;

namespace Verse
{
	public static class GenDrop
	{
		[Obsolete("Only used for mod compatibility")]
		public static bool TryDropSpawn(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
		{
			return TryDropSpawn_NewTmp(thing, dropCell, map, mode, out resultingThing, placedAction, nearPlaceValidator);
		}

		public static bool TryDropSpawn_NewTmp(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null, bool playDropSound = true)
		{
			if (map == null)
			{
				Log.Error(string.Concat("Dropped ", thing, " in a null map."));
				resultingThing = null;
				return false;
			}
			if (!dropCell.InBounds(map))
			{
				Log.Error(string.Concat("Dropped ", thing, " out of bounds at ", dropCell));
				resultingThing = null;
				return false;
			}
			if (thing.def.destroyOnDrop)
			{
				thing.Destroy();
				resultingThing = null;
				return true;
			}
			if (playDropSound && thing.def.soundDrop != null)
			{
				thing.def.soundDrop.PlayOneShot(new TargetInfo(dropCell, map));
			}
			return GenPlace.TryPlaceThing(thing, dropCell, map, mode, out resultingThing, placedAction, nearPlaceValidator);
		}
	}
}
