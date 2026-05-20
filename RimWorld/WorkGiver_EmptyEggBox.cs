using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_EmptyEggBox : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.EggBox);

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!t.Spawned)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
		if (compEggContainer?.ContainedThing == null)
		{
			return false;
		}
		if (!compEggContainer.CanEmpty && !forced)
		{
			return false;
		}
		if (!StoreUtility.TryFindBestBetterStorageFor(compEggContainer.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var _, out var _, needAccurateResult: false))
		{
			JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
		if (compEggContainer?.ContainedThing == null)
		{
			return null;
		}
		if (!compEggContainer.CanEmpty && !forced)
		{
			return null;
		}
		if (!StoreUtility.TryFindBestBetterStorageFor(compEggContainer.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell, out var _))
		{
			JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.EmptyThingContainer, t, compEggContainer.ContainedThing, foundCell);
		job.count = compEggContainer.ContainedThing.stackCount;
		return job;
	}
}
