using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class SkyfallerMaker
	{
		public static Skyfaller MakeSkyfaller(ThingDef skyfaller)
		{
			return (Skyfaller)ThingMaker.MakeThing(skyfaller);
		}

		public static Skyfaller MakeSkyfaller(ThingDef skyfaller, ThingDef innerThing)
		{
			Thing innerThing2 = ThingMaker.MakeThing(innerThing);
			return MakeSkyfaller(skyfaller, innerThing2);
		}

		public static Skyfaller MakeSkyfaller(ThingDef skyfaller, Thing innerThing)
		{
			Skyfaller skyfaller2 = MakeSkyfaller(skyfaller);
			if (innerThing != null && !skyfaller2.innerContainer.TryAdd(innerThing))
			{
				Log.Error("Could not add " + innerThing.ToStringSafe() + " to a skyfaller.");
				innerThing.Destroy();
			}
			return skyfaller2;
		}

		public static Skyfaller MakeSkyfaller(ThingDef skyfaller, IEnumerable<Thing> things)
		{
			Skyfaller skyfaller2 = MakeSkyfaller(skyfaller);
			if (things != null)
			{
				skyfaller2.innerContainer.TryAddRangeOrTransfer(things, canMergeWithExistingStacks: false, destroyLeftover: true);
			}
			return skyfaller2;
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, IntVec3 pos, Map map)
		{
			return (Skyfaller)GenSpawn.Spawn(MakeSkyfaller(skyfaller), pos, map);
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, ThingDef innerThing, IntVec3 pos, Map map)
		{
			return (Skyfaller)GenSpawn.Spawn(MakeSkyfaller(skyfaller, innerThing), pos, map);
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, Thing innerThing, IntVec3 pos, Map map)
		{
			return (Skyfaller)GenSpawn.Spawn(MakeSkyfaller(skyfaller, innerThing), pos, map);
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, IEnumerable<Thing> things, IntVec3 pos, Map map)
		{
			return (Skyfaller)GenSpawn.Spawn(MakeSkyfaller(skyfaller, things), pos, map);
		}
	}
}
