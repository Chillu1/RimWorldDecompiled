using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_PlantsCut : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.designationsByDef[DesignationDefOf.CutPlant])
		{
			yield return item.target.Thing;
		}
		foreach (Designation item2 in pawn.Map.designationManager.designationsByDef[DesignationDefOf.HarvestPlant])
		{
			yield return item2.target.Thing;
		}
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.CutPlant))
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.HarvestPlant);
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.def.category != ThingCategory.Plant)
		{
			return null;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return null;
		}
		if (t.IsBurning())
		{
			return null;
		}
		if (!PlantUtility.PawnWillingToCutPlant_Job(t, pawn))
		{
			return null;
		}
		if (!forced && t.TryGetComp(out CompPlantPreventCutting comp) && comp.PreventCutting)
		{
			return null;
		}
		foreach (Designation item in pawn.Map.designationManager.AllDesignationsOn(t))
		{
			if (item.def == DesignationDefOf.HarvestPlant)
			{
				if (!((Plant)t).HarvestableNow)
				{
					return null;
				}
				return JobMaker.MakeJob(JobDefOf.HarvestDesignated, t);
			}
			if (item.def == DesignationDefOf.CutPlant)
			{
				return JobMaker.MakeJob(JobDefOf.CutPlantDesignated, t);
			}
		}
		return null;
	}

	public override string PostProcessedGerund(Job job)
	{
		if (job.def == JobDefOf.HarvestDesignated)
		{
			return "HarvestGerund".Translate();
		}
		if (job.def == JobDefOf.CutPlantDesignated)
		{
			return "CutGerund".Translate();
		}
		return def.gerund;
	}
}
