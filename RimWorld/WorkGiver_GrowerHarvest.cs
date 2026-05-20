using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class WorkGiver_GrowerHarvest : WorkGiver_Grower
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		Plant plant = c.GetPlant(pawn.Map);
		if (plant == null)
		{
			return false;
		}
		if (!plant.HarvestableNow || plant.LifeStage != PlantLifeStage.Mature)
		{
			return false;
		}
		if (!forced && plant.TryGetComp<CompPlantPreventCutting>(out var comp) && comp.PreventCutting)
		{
			return false;
		}
		if (!plant.CanYieldNow())
		{
			return false;
		}
		if (!plant.def.plant.autoHarvestable && !forced)
		{
			return false;
		}
		if (WorkGiver_Grower.wantedPlantDef == null)
		{
			WorkGiver_Grower.wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
		}
		if (c.GetZone(pawn.Map) is Zone_Growing { allowCut: false } && plant.def != WorkGiver_Grower.wantedPlantDef)
		{
			return false;
		}
		if (!PlantUtility.PawnWillingToCutPlant_Job(plant, pawn))
		{
			return false;
		}
		if (!pawn.CanReserve(plant, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (pawn.GetLord() != null)
		{
			return true;
		}
		return base.ShouldSkip(pawn, forced);
	}

	public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Harvest);
		Map map = pawn.Map;
		Room room = c.GetRoom(map);
		float num = 0f;
		for (int i = 0; i < 40; i++)
		{
			IntVec3 intVec = c + GenRadial.RadialPattern[i];
			if (intVec.GetRoom(map) != room || !HasJobOnCell(pawn, intVec, forced))
			{
				continue;
			}
			Plant plant = intVec.GetPlant(map);
			if (!(intVec != c) || plant.def == WorkGiver_Grower.CalculateWantedPlantDef(intVec, map))
			{
				num += plant.def.plant.harvestWork;
				if (intVec != c && num > 2400f)
				{
					break;
				}
				job.AddQueuedTarget(TargetIndex.A, plant);
			}
		}
		if (job.targetQueueA != null && job.targetQueueA.Count >= 3)
		{
			job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
		}
		return job;
	}
}
