using Verse;

namespace RimWorld
{
	public static class InfestationUtility
	{
		public static Thing SpawnTunnels(int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null)
		{
			if (!InfestationCellFinder.TryFindCell(out IntVec3 cell, map))
			{
				if (!spawnAnywhereIfNoGoodCell)
				{
					return null;
				}
				if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(delegate(IntVec3 x)
				{
					if (!x.Standable(map) || x.Fogged(map))
					{
						return false;
					}
					bool flag = false;
					int num = GenRadial.NumCellsInRadius(3f);
					for (int j = 0; j < num; j++)
					{
						IntVec3 c = x + GenRadial.RadialPattern[j];
						if (c.InBounds(map))
						{
							RoofDef roof = c.GetRoof(map);
							if (roof != null && roof.isThickRoof)
							{
								flag = true;
								break;
							}
						}
					}
					return flag ? true : false;
				}, map, out cell))
				{
					return null;
				}
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
			QuestUtility.AddQuestTag(thing, questTag);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, allowUnreachable: true);
				if (cell.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
	}
}
