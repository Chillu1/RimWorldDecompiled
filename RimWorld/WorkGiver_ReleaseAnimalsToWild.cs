using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ReleaseAnimalsToWild : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.ReleaseAnimalToWild))
		{
			yield return item.target.Thing;
		}
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.ReleaseAnimalToWild);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn { IsAnimal: not false } pawn2))
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.ReleaseAnimalToWild) == null)
		{
			return false;
		}
		if (pawn.Faction != t.Faction)
		{
			return false;
		}
		if (pawn2.InAggroMentalState || pawn2.Dead)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (!JobDriver_ReleaseAnimalToWild.TryFindClosestOutsideCell(t.Position, t.Map, TraverseParms.For(pawn), pawn, out var _))
		{
			JobFailReason.Is("NoReachableOutsideCell".Translate());
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Job job = JobMaker.MakeJob(JobDefOf.ReleaseAnimalToWild, t);
		job.count = 1;
		return job;
	}
}
