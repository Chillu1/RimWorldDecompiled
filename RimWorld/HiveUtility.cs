using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class HiveUtility
{
	private const float HivePreventsClaimingInRadius = 2f;

	public static int TotalSpawnedHivesCount(Map map, bool filterFogged = false)
	{
		List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.Hive);
		if (filterFogged)
		{
			return list.Count((Thing h) => !h.Position.Fogged(h.Map));
		}
		return list.Count;
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
				Thing thing = thingList[j];
				if (Faction.OfInsects != null && thing.Faction == Faction.OfInsects && thing.def.Claimable && !AnyHivePreventsClaiming(thing) && !(thing is Pawn))
				{
					thing.SetFaction(null);
				}
			}
		}
	}

	public static Hive SpawnHive(IntVec3 spawnCell, Map map, WipeMode wipeMode = WipeMode.VanishOrMoveAside, bool spawnInsectsImmediately = false, bool canSpawnHives = true, bool canSpawnInsects = true, bool dormant = false, bool aggressive = true, bool spawnJellyImmediately = false, bool spawnSludge = true)
	{
		Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive), spawnCell, map, wipeMode);
		hive.SetFaction(Faction.OfInsects);
		hive.PawnSpawner.aggressive = aggressive;
		if (dormant)
		{
			hive.CompDormant.ToSleep();
		}
		if (spawnInsectsImmediately)
		{
			hive.PawnSpawner.SpawnPawnsUntilPoints(Rand.Range(200f, 500f));
			hive.CompDormant.WakeUp();
		}
		if (spawnJellyImmediately)
		{
			foreach (CompSpawner comp in hive.GetComps<CompSpawner>())
			{
				if (comp.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
				{
					comp.TryDoSpawn();
					break;
				}
			}
		}
		hive.PawnSpawner.canSpawnPawns = canSpawnInsects;
		hive.GetComp<CompSpawnerHives>().canSpawnHives = canSpawnHives;
		SpawnExtras(hive, map, wipeMode, spawnSludge);
		return hive;
	}

	public static void SpawnHives(IntVec3 spawnCell, Map map, int count, float radius, WipeMode wipeMode = WipeMode.VanishOrMoveAside, bool spawnInsectsImmediately = false, bool canSpawnHives = true, bool canSpawnInsects = true, bool dormant = false, bool aggressive = true, bool spawnSludge = true)
	{
		Hive hive = SpawnHive(spawnCell, map, wipeMode, spawnInsectsImmediately, canSpawnHives, canSpawnInsects, dormant, aggressive);
		for (int i = 0; i < count - 1; i++)
		{
			if (CellFinder.TryFindRandomReachableNearbyCell(spawnCell, map, radius, TraverseMode.NoPassClosedDoorsOrWater, (IntVec3 cell) => cell.Standable(map), null, out var result) && hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(result, out var newHive))
			{
				if (dormant)
				{
					hive.CompDormant.ToSleep();
				}
				hive.PawnSpawner.aggressive = aggressive;
				hive.PawnSpawner.canSpawnPawns = canSpawnInsects;
				hive.GetComp<CompSpawnerHives>().canSpawnHives = canSpawnHives;
				SpawnExtras(newHive, map, wipeMode, spawnSludge);
			}
		}
	}

	private static void SpawnExtras(Hive hive, Map map, WipeMode wipeMode, bool spawnSludge = true)
	{
		if (ModsConfig.OdysseyActive && spawnSludge)
		{
			foreach (IntVec3 item in GridShapeMaker.IrregularLump(hive.Position, map, 30, (IntVec3 c) => true))
			{
				if (item.GetEdifice(map) == null && map.reachability.CanReach(item, hive.Position, PathEndMode.OnCell, TraverseMode.PassDoors) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.InsectSludge, item, map, Rot4.North))
				{
					if (map.terrainGrid.TopTerrainAt(item).IsFloor)
					{
						map.terrainGrid.RemoveTopLayer(item, wipeMode == WipeMode.FullRefund);
					}
					map.terrainGrid.SetTerrain(item, TerrainDefOf.InsectSludge);
					if (Rand.Chance(0.2f))
					{
						GenSpawn.Spawn(ThingDefOf.Filth_Slime, item, map);
					}
				}
			}
		}
		int num = Rand.RangeInclusive(0, 2);
		for (int num2 = 0; num2 < num; num2++)
		{
			if (CellFinder.TryFindRandomReachableNearbyCell(hive.Position, map, 5f, TraverseMode.NoPassClosedDoorsOrWater, CellValidator, null, out var result))
			{
				GenSpawn.Spawn(ThingDefOf.GlowPod, result, map, wipeMode).SetFaction(Faction.OfInsects);
			}
		}
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		int num3 = Rand.RangeInclusive(0, 1);
		for (int num4 = 0; num4 < num3; num4++)
		{
			if (CellFinder.TryFindRandomReachableNearbyCell(hive.Position, map, 5f, TraverseMode.NoPassClosedDoorsOrWater, CellValidator, null, out var result2))
			{
				GenSpawn.Spawn(ThingDefOf.EggSac, result2, map, wipeMode).SetFaction(Faction.OfInsects);
			}
		}
		bool CellValidator(IntVec3 cell)
		{
			if (GenSpawn.CanSpawnAt(ThingDefOf.GlowPod, cell, map) && cell.Standable(map))
			{
				return cell.GetFirstThing<TunnelHiveSpawner>(map) == null;
			}
			return false;
		}
	}
}
