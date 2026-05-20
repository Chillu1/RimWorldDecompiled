using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_PlantSeed : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Seed);

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!ModsConfig.IdeologyActive && !ModsConfig.BiotechActive && !ModsConfig.AnomalyActive)
		{
			return !ModsConfig.OdysseyActive;
		}
		return false;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompPlantable compPlantable = t.TryGetComp<CompPlantable>();
		if (compPlantable == null || !pawn.CanReserve(t, 1, 1, null, forced))
		{
			return false;
		}
		List<IntVec3> plantCells = compPlantable.PlantCells;
		for (int i = 0; i < plantCells.Count; i++)
		{
			if (!plantCells[i].IsForbidden(pawn) && pawn.CanReach(plantCells[i], PathEndMode.Touch, Danger.Deadly) && (compPlantable.Props.plantDefToSpawn.plant.vacuumResistant || !(plantCells[i].GetVacuum(t.Map) >= 0.5f)))
			{
				Plant plant = plantCells[i].GetPlant(t.Map);
				if (plant == null || CanDoCutJob(pawn, plant, forced))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompPlantable compPlantable = t.TryGetComp<CompPlantable>();
		if (compPlantable == null)
		{
			return null;
		}
		List<IntVec3> plantCells = compPlantable.PlantCells;
		for (int i = 0; i < plantCells.Count; i++)
		{
			if (!plantCells[i].IsForbidden(pawn) && pawn.CanReach(plantCells[i], PathEndMode.Touch, Danger.Deadly) && (compPlantable.Props.plantDefToSpawn.plant.vacuumResistant || !(plantCells[i].GetVacuum(t.Map) >= 0.5f)))
			{
				Plant plant = plantCells[i].GetPlant(t.Map);
				if (plant == null)
				{
					Job job = JobMaker.MakeJob(JobDefOf.PlantSeed, t, plantCells[i]);
					job.playerForced = forced;
					job.plantDefToSow = compPlantable.Props.plantDefToSpawn;
					job.count = 1;
					return job;
				}
				if (CanDoCutJob(pawn, plant, forced))
				{
					return JobMaker.MakeJob(JobDefOf.CutPlant, plant);
				}
			}
		}
		return null;
	}

	private bool CanDoCutJob(Pawn pawn, Thing plant, bool forced)
	{
		if (!pawn.CanReserve(plant, 1, -1, null, forced))
		{
			return false;
		}
		if (!PlantUtility.PawnWillingToCutPlant_Job(plant, pawn))
		{
			return false;
		}
		return true;
	}
}
