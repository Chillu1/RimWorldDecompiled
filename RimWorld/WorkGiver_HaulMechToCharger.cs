using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HaulMechToCharger : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Haul mech to recharger"))
		{
			return false;
		}
		if (!(t is Pawn { Spawned: not false } pawn2) || !pawn2.RaceProps.IsMechanoid || !pawn2.IsColonyMech)
		{
			return false;
		}
		if (pawn2.needs.energy != null)
		{
			if (!pawn2.Downed && !pawn2.needs.energy.IsLowEnergySelfShutdown)
			{
				return false;
			}
			MechanitorControlGroup mechControlGroup = pawn2.GetMechControlGroup();
			if (mechControlGroup != null && mechControlGroup.WorkMode == MechWorkModeDefOf.SelfShutdown)
			{
				return false;
			}
			if (pawn2.needs.energy.CurLevel >= JobGiver_GetEnergy.GetMaxRechargeLimit(pawn2))
			{
				return false;
			}
		}
		if (pawn2.CurJobDef == JobDefOf.MechCharge)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return JobGiver_GetEnergy_Charger.GetClosestCharger(pawn2, pawn, forced) != null;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = (Pawn)t;
		Building_MechCharger closestCharger = JobGiver_GetEnergy_Charger.GetClosestCharger(pawn2, pawn, forced);
		Job job = JobMaker.MakeJob(JobDefOf.HaulMechToCharger, pawn2, closestCharger, closestCharger.InteractionCell);
		job.count = 1;
		return job;
	}
}
