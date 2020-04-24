using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class HiveUtility
	{
		private const float HivePreventsClaimingInRadius = 2f;

		public static int TotalSpawnedHivesCount(Map map)
		{
			return map.listerThings.ThingsOfDef(ThingDefOf.Hive).Count;
		}

		public static bool AnyHivePreventsClaiming(Thing thing)
		{
			if (!thing.Spawned)
			{
				return false;
			}
			int num = GenRadial.NumCellsInRadius(2f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = thing.Position + GenRadial.RadialPattern[i];
				if (c.InBounds(thing.Map) && c.GetFirstThing<Hive>(thing.Map) != null)
				{
					return true;
				}
			}
			return false;
		}

		public static void Notify_HiveDespawned(Hive hive, Map map)
		{
			int num = GenRadial.NumCellsInRadius(2f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = hive.Position + GenRadial.RadialPattern[i];
				if (!c.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].Faction == Faction.OfInsects && !AnyHivePreventsClaiming(thingList[j]) && !(thingList[j] is Pawn))
					{
						thingList[j].SetFaction(null);
					}
				}
			}
		}
	}
}
