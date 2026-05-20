using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_RescueDowned : WorkGiver_TakeToBed
{
	private const float MinDistFromEnemy = 40f;

	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedDownedPawns;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		List<Pawn> list = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Downed && !list[i].InBed())
			{
				return false;
			}
		}
		return true;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || !HealthAIUtility.CanRescueNow(pawn, pawn2, forced))
		{
			return false;
		}
		Thing thing = null;
		if (ChildcareUtility.CanSuckle(pawn2, out var _))
		{
			if (!HealthAIUtility.ShouldSeekMedicalRest(pawn2))
			{
				return false;
			}
			if (ChildcareUtility.SafePlaceForBaby(pawn2, pawn).Thing is Building_Bed building_Bed)
			{
				thing = building_Bed;
			}
		}
		else
		{
			thing = FindBed(pawn, pawn2);
		}
		if (thing != null && pawn2.CanReserve(thing))
		{
			return true;
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = t as Pawn;
		Thing thing = FindBed(pawn, pawn2);
		Job job = JobMaker.MakeJob(JobDefOf.Rescue, pawn2, thing);
		job.count = 1;
		return job;
	}
}
