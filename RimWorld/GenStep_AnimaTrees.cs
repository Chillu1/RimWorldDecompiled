using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class GenStep_AnimaTrees : GenStep
	{
		public static readonly float Density = 1.25E-05f;

		private const int MinDistanceToEdge = 25;

		private static readonly FloatRange GrowthRange = new FloatRange(0.5f, 0.75f);

		public override int SeedPart => 647816171;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (map.Biome.isExtremeBiome)
			{
				return;
			}
			int num = DesiredTreeCountForMap(map);
			int num2 = 0;
			IntVec3 result;
			while (num > 0 && CellFinderLoose.TryFindRandomNotEdgeCellWith(25, (IntVec3 x) => CanSpawnAt(x, map, 0, 50), map, out result))
			{
				if (TrySpawnAt(result, map, GrowthRange.RandomInRange, out var _))
				{
					num--;
				}
				num2++;
				if (num2 > 1000)
				{
					Log.Error("Could not place anima tree; too many iterations.");
					break;
				}
			}
		}

		public static bool TrySpawnAt(IntVec3 cell, Map map, float growth, out Thing plant)
		{
			cell.GetPlant(map)?.Destroy();
			plant = GenSpawn.Spawn(ThingDefOf.Plant_TreeAnima, cell, map);
			((Plant)plant).Growth = growth;
			return plant != null;
		}

		public static bool CanSpawnAt(IntVec3 c, Map map, int minProximityToArtificialStructures = 40, int minProximityToCenter = 0, int minFertileUnroofedCells = 22, int maxFertileUnroofedCellRadius = 10)
		{
			if (!c.Standable(map) || c.Fogged(map) || !c.GetRoom(map).PsychologicallyOutdoors)
			{
				return false;
			}
			Plant plant = c.GetPlant(map);
			if (plant != null && plant.def.plant.growDays > 10f)
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == ThingDefOf.Plant_TreeAnima)
				{
					return false;
				}
			}
			if (minProximityToCenter > 0 && map.Center.InHorDistOf(c, minProximityToCenter))
			{
				return false;
			}
			if (!map.reachability.CanReachFactionBase(c, map.ParentFaction))
			{
				return false;
			}
			TerrainDef terrain = c.GetTerrain(map);
			if (terrain.avoidWander || terrain.fertility <= 0f)
			{
				return false;
			}
			if (c.Roofed(map))
			{
				return false;
			}
			if (minProximityToArtificialStructures != 0 && GenRadial.RadialDistinctThingsAround(c, map, minProximityToArtificialStructures, useCenter: false).Any(MeditationUtility.CountsAsArtificialBuilding))
			{
				return false;
			}
			int num = GenRadial.NumCellsInRadius(maxFertileUnroofedCellRadius);
			int num2 = 0;
			for (int j = 0; j < num; j++)
			{
				IntVec3 intVec = c + GenRadial.RadialPattern[j];
				if (WanderUtility.InSameRoom(intVec, c, map))
				{
					if (intVec.InBounds(map) && !intVec.Roofed(map) && intVec.GetTerrain(map).fertility > 0f)
					{
						num2++;
					}
					if (num2 >= minFertileUnroofedCells)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int DesiredTreeCountForMap(Map map)
		{
			return Mathf.Max(Mathf.RoundToInt(Density * (float)map.Area), 1);
		}
	}
}
