using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class WildAnimalSpawner
{
	private Map map;

	private const int AnimalCheckInterval = 1213;

	private const float BaseAnimalSpawnChancePerInterval = 0.026955556f;

	private static readonly SimpleCurve PollutionToAnimalDensityFactorCurve = new SimpleCurve
	{
		new CurvePoint(0.1f, 1f),
		new CurvePoint(1f, 0.25f)
	};

	public static readonly SimpleCurve PollutionAnimalSpawnChanceFromPollutionCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.25f, 0.1f),
		new CurvePoint(0.75f, 0.9f),
		new CurvePoint(1f, 1f)
	};

	private float DesiredAnimalDensity
	{
		get
		{
			float animalDensity = map.TileInfo.AnimalDensity;
			float num = 0f;
			float num2 = 0f;
			foreach (BiomeDef biome in map.Biomes)
			{
				foreach (PawnKindDef allWildAnimal in biome.AllWildAnimals)
				{
					float num3 = biome.CommonalityOfAnimal(allWildAnimal);
					if (map.TileInfo.IsCoastal)
					{
						num3 += biome.CommonalityOfCoastalAnimal(allWildAnimal);
					}
					num2 += num3;
					if (map.mapTemperature.SeasonAcceptableFor(allWildAnimal.race))
					{
						num += num3;
					}
				}
			}
			animalDensity *= num / num2;
			animalDensity *= map.gameConditionManager.AggregateAnimalDensityFactor(map);
			if (ModsConfig.BiotechActive)
			{
				animalDensity *= PollutionToAnimalDensityFactorCurve.Evaluate(map.TileInfo.pollution);
			}
			return animalDensity;
		}
	}

	private float DesiredTotalAnimalWeight
	{
		get
		{
			float desiredAnimalDensity = DesiredAnimalDensity;
			if (desiredAnimalDensity == 0f)
			{
				return 0f;
			}
			float num = 10000f / desiredAnimalDensity;
			return (float)map.Area / num;
		}
	}

	private float CurrentTotalAnimalWeight
	{
		get
		{
			float num = 0f;
			IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].Faction == null)
				{
					num += allPawnsSpawned[i].kindDef.ecoSystemWeight;
				}
			}
			return num;
		}
	}

	public bool AnimalEcosystemFull => CurrentTotalAnimalWeight >= DesiredTotalAnimalWeight;

	public WildAnimalSpawner(Map map)
	{
		this.map = map;
	}

	public void WildAnimalSpawnerTick()
	{
		if (Find.TickManager.TicksGame % 1213 == 0 && !AnimalEcosystemFull && Rand.Chance(0.026955556f * DesiredAnimalDensity) && RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal, allowFogged: true, (IntVec3 cell) => map.reachability.CanReachMapEdge(cell, TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true))))
		{
			SpawnRandomWildAnimalAt(result, canFlyIn: true);
		}
	}

	public bool SpawnRandomWildAnimalAt(IntVec3 loc, bool canFlyIn, PawnKindDef animalKind = null)
	{
		if (animalKind == null && !map.BiomeAt(loc).AllWildAnimals.Where((PawnKindDef a) => map.mapTemperature.SeasonAcceptableFor(a.race)).TryRandomElementByWeight((PawnKindDef kind) => CommonalityOfAnimalNow(kind, loc), out animalKind))
		{
			return false;
		}
		bool flag = canFlyIn && animalKind.RaceProps.canFlyIntoMap && map != MapGenerator.mapBeingGenerated;
		if (flag)
		{
			bool anyWaterForSeekers = animalKind.RaceProps.waterSeeker && map.terrainGrid.AnyWaterCells;
			if (CellFinderLoose.TryGetRandomCellWith((IntVec3 c) => c.Walkable(map) && !c.Fogged(map) && !c.Roofed(map) && c.GetEdifice(map) == null && (!anyWaterForSeekers || map.terrainGrid.WaterAt(c)) && map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.ByPawn, Danger.Some)), map, 1000, out var result))
			{
				loc = result;
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (map.TileInfo.IsCoastal && map.BiomeAt(loc).ShouldSpawnAnimalOnCoast(animalKind))
		{
			TerrainGrid terrainGrid = map.terrainGrid;
			if (RCellFinder.TryFindRandomPawnEntryCell(out var result2, map, CellFinder.EdgeRoadChance_Ignore, allowFogged: true, (IntVec3 cell) => terrainGrid.TerrainAt(cell) == TerrainDefOf.WaterOceanShallow && map.reachability.CanReachMapEdge(cell, TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true))))
			{
				loc = result2;
			}
		}
		int randomInRange = animalKind.wildGroupSize.RandomInRange;
		int radius = Mathf.CeilToInt(Mathf.Sqrt(animalKind.wildGroupSize.max));
		for (int num = 0; num < randomInRange; num++)
		{
			IntVec3 intVec = CellFinder.RandomClosewalkCellNear(loc, map, radius);
			Pawn pawn = PawnGenerator.GeneratePawn(animalKind);
			if (Rand.Chance(map.BiomeAt(loc).wildAnimalScariaChance))
			{
				pawn.health.AddHediff(HediffDefOf.Scaria);
			}
			if (flag)
			{
				GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(ThingDefOf.FlyerArrival, pawn), intVec, map, ThingPlaceMode.Near);
				pawn.Rotation = Rot4.East;
			}
			else
			{
				GenSpawn.Spawn(pawn, intVec, map);
			}
		}
		return true;
	}

	private float CommonalityOfAnimalNow(PawnKindDef def, IntVec3 loc)
	{
		if (def.RaceProps.waterSeeker && !map.terrainGrid.AnyWaterCells)
		{
			return 0f;
		}
		float num = ((!ModsConfig.BiotechActive || !(Rand.Value < PollutionAnimalSpawnChanceFromPollutionCurve.Evaluate(Find.WorldGrid[map.Tile].pollution))) ? map.BiomeAt(loc).CommonalityOfAnimal(def) : map.BiomeAt(loc).CommonalityOfPollutionAnimal(def));
		if (map.TileInfo.IsCoastal)
		{
			num += map.BiomeAt(loc).CommonalityOfCoastalAnimal(def);
		}
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			if (mutator.Worker != null)
			{
				num *= mutator.Worker.AnimalCommonalityFactorFor(def, map.Tile);
			}
		}
		return num / def.wildGroupSize.Average;
	}

	public string DebugString()
	{
		return "DesiredTotalAnimalWeight: " + DesiredTotalAnimalWeight + "\nCurrentTotalAnimalWeight: " + CurrentTotalAnimalWeight + "\nDesiredAnimalDensity: " + DesiredAnimalDensity;
	}
}
