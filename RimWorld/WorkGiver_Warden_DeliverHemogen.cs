using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_DeliverHemogen : WorkGiver_Warden
{
	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModsConfig.BiotechActive)
		{
			return null;
		}
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return null;
		}
		Pawn prisoner = (Pawn)t;
		if (!prisoner.guest.CanBeBroughtFood || !prisoner.Position.IsInPrisonCell(prisoner.Map))
		{
			return null;
		}
		if (WardenFeedUtility.ShouldBeFed(prisoner))
		{
			return null;
		}
		if (!(prisoner.genes?.GetGene(GeneDefOf.Hemogenic) is Gene_Hemogen gene_Hemogen))
		{
			return null;
		}
		if (!gene_Hemogen.hemogenPacksAllowed)
		{
			return null;
		}
		if (!gene_Hemogen.ShouldConsumeHemogenNow())
		{
			return null;
		}
		if (HemogenPackAlreadyAvailableFor(prisoner))
		{
			return null;
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.HemogenPack), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack) && pack.GetRoom() != prisoner.GetRoom());
		if (thing == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.DeliverFood, thing, prisoner);
		job.count = 1;
		job.targetC = RCellFinder.SpotToChewStandingNear(prisoner, thing);
		return job;
	}

	private bool HemogenPackAlreadyAvailableFor(Pawn prisoner)
	{
		if (prisoner.carryTracker.CarriedCount(ThingDefOf.HemogenPack) > 0)
		{
			return true;
		}
		if (prisoner.inventory.Count(ThingDefOf.HemogenPack) > 0)
		{
			return true;
		}
		Room room = prisoner.GetRoom();
		if (room != null)
		{
			List<Region> regions = room.Regions;
			for (int i = 0; i < regions.Count; i++)
			{
				if (regions[i].ListerThings.ThingsOfDef(ThingDefOf.HemogenPack).Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}
}
