using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_GauranlenPodSpawn : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (base.CanFireNowSub(parms))
			{
				return ModsConfig.IdeologyActive;
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindRootCell(map, out var cell))
			{
				return false;
			}
			if (!TrySpawnAt(cell, map, out var plant))
			{
				return false;
			}
			((Plant)plant).Growth = 1f;
			SendStandardLetter(parms, plant);
			return true;
		}

		public static bool IsGoodBiome(BiomeDef biomeDef)
		{
			IncidentDef gauranlenPodSpawn = IncidentDefOf.GauranlenPodSpawn;
			if (gauranlenPodSpawn.disallowedBiomes != null && gauranlenPodSpawn.disallowedBiomes.Contains(biomeDef))
			{
				return false;
			}
			if (gauranlenPodSpawn.allowedBiomes != null)
			{
				return gauranlenPodSpawn.allowedBiomes.Contains(biomeDef);
			}
			return true;
		}

		private static bool CanSpawnPodAt(IntVec3 c, Map map)
		{
			if (!c.Standable(map) || c.Fogged(map) || !c.GetRoom(map).PsychologicallyOutdoors || c.Roofed(map))
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
				if (thingList[i].def == ThingDefOf.Plant_PodGauranlen)
				{
					return false;
				}
			}
			if (!map.reachability.CanReachFactionBase(c, map.ParentFaction))
			{
				return false;
			}
			if (c.GetTerrain(map).avoidWander)
			{
				return false;
			}
			if (c.GetFertility(map) < ThingDefOf.Plant_PodGauranlen.plant.fertilityMin)
			{
				return false;
			}
			return true;
		}

		public static bool TryFindRootCell(Map map, out IntVec3 cell)
		{
			return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => CanSpawnPodAt(x, map), map, out cell);
		}

		private bool TrySpawnAt(IntVec3 cell, Map map, out Thing plant)
		{
			cell.GetPlant(map)?.Destroy();
			plant = GenSpawn.Spawn(ThingDefOf.Plant_PodGauranlen, cell, map);
			return plant != null;
		}
	}
}
