using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WildAnimalSpawner
	{
		private Map map;

		private const int AnimalCheckInterval = 1213;

		private const float BaseAnimalSpawnChancePerInterval = 0.0269555561f;

		private float DesiredAnimalDensity
		{
			get
			{
				float animalDensity = map.Biome.animalDensity;
				float num = 0f;
				float num2 = 0f;
				foreach (PawnKindDef allWildAnimal in map.Biome.AllWildAnimals)
				{
					float num3 = map.Biome.CommonalityOfAnimal(allWildAnimal);
					num2 += num3;
					if (map.mapTemperature.SeasonAcceptableFor(allWildAnimal.race))
					{
						num += num3;
					}
				}
				animalDensity *= num / num2;
				return animalDensity * map.gameConditionManager.AggregateAnimalDensityFactor(map);
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
				List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
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
			if (Find.TickManager.TicksGame % 1213 == 0 && !AnimalEcosystemFull && Rand.Chance(0.0269555561f * DesiredAnimalDensity))
			{
				TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoors);
				if (RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal, allowFogged: true, (IntVec3 cell) => map.reachability.CanReachMapEdge(cell, traverseParms)))
				{
					SpawnRandomWildAnimalAt(result);
				}
			}
		}

		public bool SpawnRandomWildAnimalAt(IntVec3 loc)
		{
			PawnKindDef pawnKindDef = map.Biome.AllWildAnimals.Where((PawnKindDef a) => map.mapTemperature.SeasonAcceptableFor(a.race)).RandomElementByWeight((PawnKindDef def) => map.Biome.CommonalityOfAnimal(def) / def.wildGroupSize.Average);
			if (pawnKindDef == null)
			{
				Log.Error("No spawnable animals right now.");
				return false;
			}
			int randomInRange = pawnKindDef.wildGroupSize.RandomInRange;
			int radius = Mathf.CeilToInt(Mathf.Sqrt(pawnKindDef.wildGroupSize.max));
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, map, radius);
				GenSpawn.Spawn(PawnGenerator.GeneratePawn(pawnKindDef), loc2, map);
			}
			return true;
		}

		public string DebugString()
		{
			return "DesiredTotalAnimalWeight: " + DesiredTotalAnimalWeight + "\nCurrentTotalAnimalWeight: " + CurrentTotalAnimalWeight + "\nDesiredAnimalDensity: " + DesiredAnimalDensity;
		}
	}
}
