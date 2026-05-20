using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_RepairMech : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
	}

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !MechanitorUtility.IsMechanitor(pawn);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Repair mech"))
		{
			return false;
		}
		Pawn pawn2 = (Pawn)t;
		CompMechRepairable compMechRepairable = t.TryGetComp<CompMechRepairable>();
		if (compMechRepairable == null)
		{
			return false;
		}
		if (!pawn2.RaceProps.IsMechanoid)
		{
			return false;
		}
		if (pawn2.InAggroMentalState || pawn2.HostileTo(pawn))
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn2.IsBurning())
		{
			return false;
		}
		if (pawn2.IsAttacking())
		{
			return false;
		}
		if (pawn2.needs.energy == null)
		{
			return false;
		}
		if (!MechRepairUtility.CanRepair(pawn2))
		{
			return false;
		}
		if (!forced)
		{
			return compMechRepairable.autoRepair;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.RepairMech, t);
	}
}
