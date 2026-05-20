using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_EmptyWasteContainer : WorkGiver_Scanner
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.WasteProducer);
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.WasteProducer).Any();
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Empty waste container"))
		{
			return false;
		}
		LocalTargetInfo target = t;
		bool ignoreOtherReservations = forced;
		if (!pawn.CanReserve(target, 1, -1, GetReservationLayer(pawn, t), ignoreOtherReservations))
		{
			return false;
		}
		CompWasteProducer compWasteProducer = t.TryGetComp<CompWasteProducer>();
		if (compWasteProducer == null || !compWasteProducer.CanEmptyNow || compWasteProducer.Waste.stackCount <= 0)
		{
			return false;
		}
		if (!StoreUtility.TryFindBestBetterStorageFor(compWasteProducer.Waste, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var _, out var _, needAccurateResult: false))
		{
			JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompWasteProducer compWasteProducer = t.TryGetComp<CompWasteProducer>();
		if (compWasteProducer == null || !compWasteProducer.CanEmptyNow || compWasteProducer.Waste.stackCount <= 0)
		{
			return null;
		}
		if (!StoreUtility.TryFindBestBetterStorageFor(compWasteProducer.Waste, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell, out var _))
		{
			JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.EmptyWasteContainer, t, compWasteProducer.Waste, foundCell);
		job.count = compWasteProducer.Waste.stackCount;
		return job;
	}

	public override ReservationLayerDef GetReservationLayer(Pawn pawn, LocalTargetInfo t)
	{
		return ReservationLayerDefOf.Empty;
	}
}
