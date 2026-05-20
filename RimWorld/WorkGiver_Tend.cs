using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Tend : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedPawnsWithAnyHediff;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || !GoodLayingStatusForTend(pawn2, pawn) || !HealthAIUtility.ShouldBeTendedNowByPlayer(pawn2) || !pawn.CanReserve(pawn2, 1, -1, null, forced) || (pawn2.IsMutant && !pawn2.mutant.Def.entitledToMedicalCare) || (pawn2.InAggroMentalState && !pawn2.health.hediffSet.HasHediff(HediffDefOf.Scaria)))
		{
			return false;
		}
		return true;
	}

	public static bool GoodLayingStatusForTend(Pawn patient, Pawn doctor)
	{
		if (patient == doctor)
		{
			return true;
		}
		if (patient.RaceProps.Humanlike)
		{
			return patient.InBed();
		}
		return patient.GetPosture() != PawnPosture.Standing;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = t as Pawn;
		Thing thing = HealthAIUtility.FindBestMedicine(pawn, pawn2);
		if (thing != null && thing.SpawnedParentOrMe != thing)
		{
			return JobMaker.MakeJob(JobDefOf.TendPatient, pawn2, thing, thing.SpawnedParentOrMe);
		}
		if (thing != null)
		{
			return JobMaker.MakeJob(JobDefOf.TendPatient, pawn2, thing);
		}
		return JobMaker.MakeJob(JobDefOf.TendPatient, pawn2);
	}
}
