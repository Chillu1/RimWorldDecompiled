using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_TakeForInventoryStock : ThinkNode_JobGiver
{
	private const int InventoryStockCheckIntervalMin = 6000;

	private const int InventoryStockCheckIntervalMax = 9000;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (Find.TickManager.TicksGame < pawn.mindState.nextInventoryStockTick)
		{
			return null;
		}
		if (!pawn.inventoryStock.AnyThingsRequiredNow())
		{
			return null;
		}
		if (pawn.inventory.UnloadEverything)
		{
			return null;
		}
		foreach (InventoryStockEntry value in pawn.inventoryStock.stockEntries.Values)
		{
			if (pawn.inventory.Count(value.thingDef) < value.count)
			{
				Thing thing = FindThingFor(pawn, value.thingDef);
				if (thing != null)
				{
					Job job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, thing);
					job.count = Mathf.Min(b: value.count - pawn.inventory.Count(thing.def), a: thing.stackCount);
					return job;
				}
			}
		}
		pawn.mindState.nextInventoryStockTick = Find.TickManager.TicksGame + Rand.Range(6000, 9000);
		return null;
	}

	private Thing FindThingFor(Pawn pawn, ThingDef thingDef)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(thingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => ThingValidator(pawn, x));
	}

	private bool ThingValidator(Pawn pawn, Thing thing)
	{
		if (thing.IsForbidden(pawn))
		{
			return false;
		}
		if (!pawn.CanReserve(thing, 10))
		{
			return false;
		}
		return true;
	}
}
