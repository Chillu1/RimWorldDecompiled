using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class GenStep_Power : GenStep
{
	public bool canSpawnBatteries = true;

	public bool canSpawnPowerGenerators = true;

	public bool spawnRoofOverNewBatteries = true;

	public FloatRange newBatteriesInitialStoredEnergyPctRange = new FloatRange(0.2f, 0.5f);

	private List<Thing> tmpThings = new List<Thing>();

	private List<IntVec3> tmpCells = new List<IntVec3>();

	private const int MaxDistToExistingNetForTurrets = 13;

	private const int RoofPadding = 2;

	private static readonly IntRange MaxDistanceBetweenBatteryAndTransmitter = new IntRange(20, 50);

	private bool hasAtleast1TurretInt;

	private Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();

	private static List<IntVec3> tmpTransmitterCells = new List<IntVec3>();

	public override int SeedPart => 1186199651;

	public override void Generate(Map map, GenStepParams parms)
	{
		map.skyManager.ForceSetCurSkyGlow(1f);
		map.powerNetManager.UpdatePowerNetsAndConnections_First();
		UpdateDesiredPowerOutputForAllGenerators(map);
		EnsureBatteriesConnectedAndMakeSense(map);
		EnsurePowerUsersConnected(map);
		EnsureGeneratorsConnectedAndMakeSense(map);
		tmpThings.Clear();
	}

	private void UpdateDesiredPowerOutputForAllGenerators(Map map)
	{
		tmpThings.Clear();
		tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
		for (int i = 0; i < tmpThings.Count; i++)
		{
			if (IsPowerGenerator(tmpThings[i]))
			{
				tmpThings[i].TryGetComp<CompPowerPlant>()?.UpdateDesiredPowerOutput();
			}
		}
	}

	private void EnsureBatteriesConnectedAndMakeSense(Map map)
	{
		tmpThings.Clear();
		tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
		for (int i = 0; i < tmpThings.Count; i++)
		{
			CompPowerBattery compPowerBattery = tmpThings[i].TryGetComp<CompPowerBattery>();
			if (compPowerBattery == null)
			{
				continue;
			}
			PowerNet powerNet = compPowerBattery.PowerNet;
			if (powerNet != null && HasAnyPowerGenerator(powerNet))
			{
				continue;
			}
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
			Building newPowerGenerator2;
			if (TryFindClosestReachableNet(compPowerBattery.parent.Position, (PowerNet x) => HasAnyPowerGenerator(x), map, out var foundNet, out var closestTransmitter))
			{
				map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
				if (canSpawnPowerGenerators)
				{
					int count = tmpCells.Count;
					if (Rand.Chance(Mathf.InverseLerp(MaxDistanceBetweenBatteryAndTransmitter.min, MaxDistanceBetweenBatteryAndTransmitter.max, count)) && TrySpawnPowerGeneratorNear(compPowerBattery.parent.Position, map, compPowerBattery.parent.Faction, out var newPowerGenerator))
					{
						SpawnTransmitters(compPowerBattery.parent.Position, newPowerGenerator.Position, map, compPowerBattery.parent.Faction);
						foundNet = null;
					}
				}
				if (foundNet != null)
				{
					SpawnTransmitters(tmpCells, map, compPowerBattery.parent.Faction);
				}
			}
			else if (canSpawnPowerGenerators && TrySpawnPowerGeneratorNear(compPowerBattery.parent.Position, map, compPowerBattery.parent.Faction, out newPowerGenerator2))
			{
				SpawnTransmitters(compPowerBattery.parent.Position, newPowerGenerator2.Position, map, compPowerBattery.parent.Faction);
			}
		}
	}

	private void EnsurePowerUsersConnected(Map map)
	{
		tmpThings.Clear();
		tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
		hasAtleast1TurretInt = tmpThings.Any((Thing t) => t is Building_Turret);
		for (int num = 0; num < tmpThings.Count; num++)
		{
			if (!IsPowerUser(tmpThings[num]))
			{
				continue;
			}
			CompPowerTrader powerComp = tmpThings[num].TryGetComp<CompPowerTrader>();
			PowerNet powerNet = powerComp.PowerNet;
			if (powerNet != null && powerNet.hasPowerSource)
			{
				TryTurnOnImmediately(powerComp, map);
				continue;
			}
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
			Building newBattery;
			if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentEnergyGainRate() - powerComp.Props.PowerConsumption * CompPower.WattsToWattDaysPerTick > 1E-07f, map, out var foundNet, out var closestTransmitter))
			{
				map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
				bool flag = false;
				if (canSpawnPowerGenerators && tmpThings[num] is Building_Turret && tmpCells.Count > 13)
				{
					flag = TrySpawnPowerGeneratorAndBatteryIfCanAndConnect(tmpThings[num], map);
				}
				if (!flag)
				{
					SpawnTransmitters(tmpCells, map, tmpThings[num].Faction);
				}
				TryTurnOnImmediately(powerComp, map);
			}
			else if (canSpawnPowerGenerators && TrySpawnPowerGeneratorAndBatteryIfCanAndConnect(tmpThings[num], map))
			{
				TryTurnOnImmediately(powerComp, map);
			}
			else if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentStoredEnergy() > 1E-07f, map, out foundNet, out closestTransmitter))
			{
				map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
				SpawnTransmitters(tmpCells, map, tmpThings[num].Faction);
			}
			else if (canSpawnBatteries && TrySpawnBatteryNear(tmpThings[num].Position, map, tmpThings[num].Faction, out newBattery))
			{
				SpawnTransmitters(tmpThings[num].Position, newBattery.Position, map, tmpThings[num].Faction);
				if (newBattery.GetComp<CompPowerBattery>().StoredEnergy > 0f)
				{
					TryTurnOnImmediately(powerComp, map);
				}
			}
		}
	}

	private void EnsureGeneratorsConnectedAndMakeSense(Map map)
	{
		tmpThings.Clear();
		tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
		for (int i = 0; i < tmpThings.Count; i++)
		{
			if (!IsPowerGenerator(tmpThings[i]))
			{
				continue;
			}
			PowerNet powerNet = tmpThings[i].TryGetComp<CompPower>().PowerNet;
			if (powerNet == null || !HasAnyPowerUser(powerNet))
			{
				map.powerNetManager.UpdatePowerNetsAndConnections_First();
				if (TryFindClosestReachableNet(tmpThings[i].Position, (PowerNet x) => HasAnyPowerUser(x), map, out var _, out var closestTransmitter))
				{
					map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
					SpawnTransmitters(tmpCells, map, tmpThings[i].Faction);
				}
			}
		}
	}

	private bool IsPowerUser(Thing thing)
	{
		CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
		if (compPowerTrader != null)
		{
			if (!(compPowerTrader.PowerOutput < 0f))
			{
				if (!compPowerTrader.PowerOn)
				{
					return compPowerTrader.Props.PowerConsumption > 0f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool IsPowerGenerator(Thing thing)
	{
		if (thing.TryGetComp<CompPowerPlant>() != null)
		{
			return true;
		}
		CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
		if (compPowerTrader != null)
		{
			if (!(compPowerTrader.PowerOutput > 0f))
			{
				if (!compPowerTrader.PowerOn)
				{
					return compPowerTrader.Props.PowerConsumption < 0f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool HasAnyPowerGenerator(PowerNet net)
	{
		List<CompPowerTrader> powerComps = net.powerComps;
		for (int i = 0; i < powerComps.Count; i++)
		{
			if (IsPowerGenerator(powerComps[i].parent))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasAnyPowerUser(PowerNet net)
	{
		List<CompPowerTrader> powerComps = net.powerComps;
		for (int i = 0; i < powerComps.Count; i++)
		{
			if (IsPowerUser(powerComps[i].parent))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryFindClosestReachableNet(IntVec3 root, Predicate<PowerNet> predicate, Map map, out PowerNet foundNet, out IntVec3 closestTransmitter)
	{
		tmpPowerNetPredicateResults.Clear();
		PowerNet foundNetLocal = null;
		IntVec3 closestTransmitterLocal = IntVec3.Invalid;
		map.floodFiller.FloodFill(root, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate(IntVec3 x)
		{
			PowerNet powerNet = x.GetTransmitter(map)?.GetComp<CompPower>().PowerNet;
			if (powerNet == null)
			{
				return false;
			}
			if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out var value))
			{
				value = predicate(powerNet);
				tmpPowerNetPredicateResults.Add(powerNet, value);
			}
			if (value)
			{
				foundNetLocal = powerNet;
				closestTransmitterLocal = x;
				return true;
			}
			return false;
		}, int.MaxValue, rememberParents: true);
		tmpPowerNetPredicateResults.Clear();
		if (foundNetLocal != null)
		{
			foundNet = foundNetLocal;
			closestTransmitter = closestTransmitterLocal;
			return true;
		}
		foundNet = null;
		closestTransmitter = IntVec3.Invalid;
		return false;
	}

	private void SpawnTransmitters(List<IntVec3> cells, Map map, Faction faction)
	{
		for (int i = 0; i < cells.Count; i++)
		{
			if (cells[i].GetTransmitter(map) == null)
			{
				GenSpawn.Spawn(ThingDefOf.PowerConduit, cells[i], map).SetFaction(faction);
			}
		}
	}

	private void SpawnTransmitters(IntVec3 start, IntVec3 end, Map map, Faction faction)
	{
		bool foundPath = false;
		map.floodFiller.FloodFill(start, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate(IntVec3 x)
		{
			if (x == end)
			{
				foundPath = true;
				return true;
			}
			return false;
		}, int.MaxValue, rememberParents: true);
		if (foundPath)
		{
			map.floodFiller.ReconstructLastFloodFillPath(end, tmpTransmitterCells);
			SpawnTransmitters(tmpTransmitterCells, map, faction);
		}
	}

	private bool TrySpawnPowerTransmittingBuildingNear(IntVec3 position, Map map, Faction faction, ThingDef def, out Building newBuilding, Predicate<IntVec3> extraValidator = null)
	{
		TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassAllDestroyableThings);
		if (RCellFinder.TryFindRandomCellNearWith(position, delegate(IntVec3 x)
		{
			if (!x.Standable(map) || x.Roofed(map) || !EverPossibleToTransmitPowerAt(x, map))
			{
				return false;
			}
			if (!map.reachability.CanReach(position, x, PathEndMode.OnCell, traverseParams))
			{
				return false;
			}
			foreach (IntVec3 item in GenAdj.OccupiedRect(x, Rot4.North, def.size))
			{
				if (!item.InBounds(map) || item.Roofed(map) || item.GetEdifice(map) != null || item.GetFirstItem(map) != null || item.GetTransmitter(map) != null)
				{
					return false;
				}
			}
			return (extraValidator == null || extraValidator(x)) ? true : false;
		}, map, out var result, 8))
		{
			newBuilding = (Building)GenSpawn.Spawn(ThingMaker.MakeThing(def), result, map, Rot4.North);
			newBuilding.SetFaction(faction);
			return true;
		}
		newBuilding = null;
		return false;
	}

	private bool TrySpawnPowerGeneratorNear(IntVec3 position, Map map, Faction faction, out Building newPowerGenerator)
	{
		if (TrySpawnPowerTransmittingBuildingNear(position, map, faction, ThingDefOf.SolarGenerator, out newPowerGenerator))
		{
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
			newPowerGenerator.GetComp<CompPowerPlant>().UpdateDesiredPowerOutput();
			return true;
		}
		return false;
	}

	private bool TrySpawnBatteryNear(IntVec3 position, Map map, Faction faction, out Building newBattery)
	{
		Predicate<IntVec3> extraValidator = null;
		if (spawnRoofOverNewBatteries)
		{
			extraValidator = delegate(IntVec3 x)
			{
				foreach (IntVec3 item in GenAdj.OccupiedRect(x, Rot4.North, ThingDefOf.Battery.size).ExpandedBy(3))
				{
					if (item.InBounds(map))
					{
						List<Thing> thingList = item.GetThingList(map);
						for (int i = 0; i < thingList.Count; i++)
						{
							if (thingList[i].def.PlaceWorkers != null && thingList[i].def.PlaceWorkers.Any((PlaceWorker y) => y is PlaceWorker_NotUnderRoof))
							{
								return false;
							}
						}
					}
				}
				return true;
			};
		}
		if (TrySpawnPowerTransmittingBuildingNear(position, map, faction, ThingDefOf.Battery, out newBattery, extraValidator))
		{
			float randomInRange = newBatteriesInitialStoredEnergyPctRange.RandomInRange;
			newBattery.GetComp<CompPowerBattery>().SetStoredEnergyPct(randomInRange);
			if (spawnRoofOverNewBatteries)
			{
				SpawnRoofOver(newBattery);
			}
			return true;
		}
		return false;
	}

	private bool TrySpawnPowerGeneratorAndBatteryIfCanAndConnect(Thing forThing, Map map)
	{
		if (!canSpawnPowerGenerators)
		{
			return false;
		}
		IntVec3 position = forThing.Position;
		if (canSpawnBatteries && Rand.Chance(hasAtleast1TurretInt ? 1f : 0.1f) && TrySpawnBatteryNear(forThing.Position, map, forThing.Faction, out var newBattery))
		{
			SpawnTransmitters(forThing.Position, newBattery.Position, map, forThing.Faction);
			position = newBattery.Position;
		}
		if (TrySpawnPowerGeneratorNear(position, map, forThing.Faction, out var newPowerGenerator))
		{
			SpawnTransmitters(position, newPowerGenerator.Position, map, forThing.Faction);
			return true;
		}
		return false;
	}

	private bool EverPossibleToTransmitPowerAt(IntVec3 c, Map map)
	{
		if (c.GetTransmitter(map) == null)
		{
			return GenConstruct.CanBuildOnTerrain(ThingDefOf.PowerConduit, c, map, Rot4.North);
		}
		return true;
	}

	private void TryTurnOnImmediately(CompPowerTrader powerComp, Map map)
	{
		if (!powerComp.PowerOn)
		{
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
			if (powerComp.PowerNet != null && powerComp.PowerNet.CurrentEnergyGainRate() > 1E-07f)
			{
				powerComp.PowerOn = true;
			}
		}
	}

	private void SpawnRoofOver(Thing thing)
	{
		CellRect cellRect = thing.OccupiedRect();
		bool flag = true;
		foreach (IntVec3 item in cellRect)
		{
			if (!item.Roofed(thing.Map))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		int num = 0;
		CellRect cellRect2 = cellRect.ExpandedBy(2);
		foreach (IntVec3 item2 in cellRect2)
		{
			if (item2.InBounds(thing.Map) && item2.GetRoofHolderOrImpassable(thing.Map) != null)
			{
				num++;
			}
		}
		if (num < 2)
		{
			ThingDef stuff = Rand.Element(ThingDefOf.WoodLog, ThingDefOf.Steel);
			foreach (IntVec3 corner in cellRect2.Corners)
			{
				if (corner.InBounds(thing.Map) && corner.Standable(thing.Map) && corner.GetFirstItem(thing.Map) == null && corner.GetFirstBuilding(thing.Map) == null && corner.GetFirstPawn(thing.Map) == null && !GenAdj.CellsAdjacent8Way(new TargetInfo(corner, thing.Map)).Any((IntVec3 x) => !x.InBounds(thing.Map) || !x.Walkable(thing.Map)) && corner.SupportsStructureType(thing.Map, ThingDefOf.Wall.terrainAffordanceNeeded))
				{
					Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Wall, stuff);
					GenSpawn.Spawn(thing2, corner, thing.Map);
					thing2.SetFaction(thing.Faction);
					num++;
				}
			}
		}
		if (num <= 0)
		{
			return;
		}
		foreach (IntVec3 item3 in cellRect2)
		{
			if (item3.InBounds(thing.Map) && !item3.Roofed(thing.Map))
			{
				thing.Map.roofGrid.SetRoof(item3, RoofDefOf.RoofConstructed);
			}
		}
	}
}
