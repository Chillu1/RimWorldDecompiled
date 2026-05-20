using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_CreateXenogerm : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.GeneAssembler);

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.BiotechActive;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building_GeneAssembler building_GeneAssembler))
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced) || !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced))
		{
			return false;
		}
		if (building_GeneAssembler.ArchitesRequiredNow > 0)
		{
			if (FindArchiteCapsule(pawn) == null)
			{
				JobFailReason.Is("NoIngredient".Translate(ThingDefOf.ArchiteCapsule));
				return false;
			}
			return true;
		}
		if (!building_GeneAssembler.CanBeWorkedOnNow.Accepted)
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building_GeneAssembler building_GeneAssembler))
		{
			return null;
		}
		if (building_GeneAssembler.ArchitesRequiredNow > 0)
		{
			Thing thing = FindArchiteCapsule(pawn);
			if (thing != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, thing, t);
				job.count = Mathf.Min(building_GeneAssembler.ArchitesRequiredNow, thing.stackCount);
				return job;
			}
		}
		return JobMaker.MakeJob(JobDefOf.CreateXenogerm, t, 1200, checkOverrideOnExpiry: true);
	}

	private Thing FindArchiteCapsule(Pawn pawn)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.ArchiteCapsule), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x));
	}
}
