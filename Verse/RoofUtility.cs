using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse;

public static class RoofUtility
{
	public static Thing FirstBlockingThing(IntVec3 pos, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(pos);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.plant != null && list[i].def.plant.interferesWithRoof)
			{
				return list[i];
			}
		}
		return null;
	}

	public static bool IsAnyCellUnderRoof(Thing thing)
	{
		CellRect cellRect = thing.OccupiedRect();
		bool result = false;
		RoofGrid roofGrid = thing.Map.roofGrid;
		foreach (IntVec3 item in cellRect)
		{
			if (roofGrid.Roofed(item))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool CanHandleBlockingThing(Thing blocker, Pawn worker, bool forced = false)
	{
		if (blocker == null)
		{
			return true;
		}
		if (blocker.def.category == ThingCategory.Plant)
		{
			if (!PlantUtility.PawnWillingToCutPlant_Job(blocker, worker))
			{
				return false;
			}
			if (worker.CanReserveAndReach(blocker, PathEndMode.ClosestTouch, worker.NormalMaxDanger(), 1, -1, null, forced))
			{
				return true;
			}
		}
		return false;
	}

	public static Job HandleBlockingThingJob(Thing blocker, Pawn worker, bool forced = false)
	{
		if (blocker == null)
		{
			return null;
		}
		if (blocker.def.category == ThingCategory.Plant && worker.CanReserveAndReach(blocker, PathEndMode.ClosestTouch, worker.NormalMaxDanger(), 1, -1, null, forced))
		{
			if (!PlantUtility.PawnWillingToCutPlant_Job(blocker, worker))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.CutPlant, blocker);
		}
		return null;
	}
}
