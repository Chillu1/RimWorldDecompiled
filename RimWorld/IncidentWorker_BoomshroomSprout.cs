using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_BoomshroomSprout : IncidentWorker
{
	private static readonly IntRange CountRange = new IntRange(6, 10);

	private const int MinRoomCells = 64;

	private const int SpawnRadius = 4;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (!PlantUtility.GrowthSeasonNow(map, ThingDefOf.Boomshroom))
		{
			return false;
		}
		IntVec3 cell;
		return TryFindRootCell(map, out cell);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!TryFindRootCell(map, out var cell))
		{
			return false;
		}
		Thing thing = null;
		int randomInRange = CountRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (!CellFinder.TryRandomClosewalkCellNear(cell, map, 4, out var result, (IntVec3 x) => CanSpawnAt(x, map)))
			{
				break;
			}
			result.GetPlant(map)?.Destroy();
			Thing thing2 = GenSpawn.Spawn(ThingDefOf.Boomshroom, result, map);
			if (thing == null)
			{
				thing = thing2;
			}
		}
		if (thing == null)
		{
			return false;
		}
		SendStandardLetter(parms, thing);
		return true;
	}

	private bool TryFindRootCell(Map map, out IntVec3 cell)
	{
		return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => CanSpawnAt(x, map) && x.GetRoom(map).CellCount >= 64, map, out cell);
	}

	private bool CanSpawnAt(IntVec3 c, Map map)
	{
		if (!c.Standable(map) || c.Fogged(map) || map.fertilityGrid.FertilityAt(c) < ThingDefOf.Boomshroom.plant.fertilityMin || !c.GetRoom(map).PsychologicallyOutdoors || c.GetEdifice(map) != null || !PlantUtility.GrowthSeasonNow(c, map, ThingDefOf.Boomshroom))
		{
			return false;
		}
		if (!ThingDefOf.Boomshroom.CanEverPlantAt(c, map, out var _, canWipePlantsExceptTree: false, checkMapTemperature: true, writeNoReason: true))
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
			if (thingList[i].def == ThingDefOf.Boomshroom)
			{
				return false;
			}
		}
		return true;
	}
}
