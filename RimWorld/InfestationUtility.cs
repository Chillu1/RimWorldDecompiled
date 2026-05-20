using System;
using Verse;

namespace RimWorld;

public static class InfestationUtility
{
	private static IntVec3 FindRootTunnelLoc(Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofIfNoGoodCell = false)
	{
		if (InfestationCellFinder.TryFindCell(out var cell, map))
		{
			return cell;
		}
		if (!spawnAnywhereIfNoGoodCell)
		{
			return IntVec3.Invalid;
		}
		Func<IntVec3, bool, bool> validator = delegate(IntVec3 x, bool canIgnoreRoof)
		{
			if (!x.Standable(map) || x.Fogged(map))
			{
				return false;
			}
			if (!canIgnoreRoof)
			{
				bool flag = false;
				int num = GenRadial.NumCellsInRadius(3f);
				for (int i = 0; i < num; i++)
				{
					IntVec3 c = x + GenRadial.RadialPattern[i];
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
				if (!flag)
				{
					return false;
				}
			}
			return true;
		};
		if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => validator(x, arg2: false), map, out cell))
		{
			return cell;
		}
		if (ignoreRoofIfNoGoodCell && RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => validator(x, arg2: true), map, out cell))
		{
			return cell;
		}
		return IntVec3.Invalid;
	}

	public static Thing SpawnTunnels(int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null, IntVec3? overrideLoc = null, float? insectsPoints = null)
	{
		IntVec3 loc = (overrideLoc.HasValue ? overrideLoc.Value : default(IntVec3));
		if (!overrideLoc.HasValue)
		{
			loc = FindRootTunnelLoc(map, spawnAnywhereIfNoGoodCell);
		}
		if (!loc.IsValid)
		{
			return null;
		}
		TunnelHiveSpawner tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner);
		Thing thing = GenSpawn.Spawn(tunnelHiveSpawner, loc, map, WipeMode.FullRefund);
		if (insectsPoints.HasValue)
		{
			tunnelHiveSpawner.insectsPoints = insectsPoints.Value;
		}
		QuestUtility.AddQuestTag(thing, questTag);
		for (int i = 0; i < hiveCount - 1; i++)
		{
			loc = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, allowUnreachable: true);
			if (loc.IsValid)
			{
				tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner);
				thing = GenSpawn.Spawn(tunnelHiveSpawner, loc, map, WipeMode.FullRefund);
				if (insectsPoints.HasValue)
				{
					tunnelHiveSpawner.insectsPoints = insectsPoints.Value;
				}
				QuestUtility.AddQuestTag(thing, questTag);
			}
		}
		return thing;
	}

	public static Thing SpawnJellyTunnels(int tunnelCount, int jellyCount, Map map)
	{
		IntVec3 loc = FindRootTunnelLoc(map, spawnAnywhereIfNoGoodCell: true, ignoreRoofIfNoGoodCell: true);
		if (!loc.IsValid)
		{
			return null;
		}
		int num = jellyCount;
		TunnelJellySpawner tunnelJellySpawner = (TunnelJellySpawner)ThingMaker.MakeThing(ThingDefOf.TunnelJellySpawner);
		tunnelJellySpawner.jellyCount = jellyCount / tunnelCount;
		num -= tunnelJellySpawner.jellyCount;
		GenSpawn.Spawn(tunnelJellySpawner, loc, map, WipeMode.FullRefund);
		for (int i = 0; i < tunnelCount - 1; i++)
		{
			loc = CompSpawnerHives.FindChildHiveLocation(tunnelJellySpawner.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement: true, allowUnreachable: false);
			if (loc.IsValid)
			{
				tunnelJellySpawner = (TunnelJellySpawner)ThingMaker.MakeThing(ThingDefOf.TunnelJellySpawner);
				if (i < tunnelCount - 2)
				{
					tunnelJellySpawner.jellyCount = jellyCount / tunnelCount;
					num -= tunnelJellySpawner.jellyCount;
				}
				else
				{
					tunnelJellySpawner.jellyCount = num;
				}
				GenSpawn.Spawn(tunnelJellySpawner, loc, map, WipeMode.FullRefund);
			}
		}
		return tunnelJellySpawner;
	}
}
