using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Tame : WorkGiver_InteractAnimal
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Tame))
		{
			yield return item.target.Thing;
		}
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Tame);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || !TameUtility.CanTame(pawn2))
		{
			return null;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Tame) == null)
		{
			return null;
		}
		if (!CanInteractWithAnimal(pawn, pawn2, forced))
		{
			return null;
		}
		if (TameUtility.TriedToTameTooRecently(pawn2))
		{
			JobFailReason.Is(WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans);
			return null;
		}
		Thing thing = null;
		int count = -1;
		if (pawn2.RaceProps.EatsFood && pawn2.needs?.food != null && !HasFoodToInteractAnimal(pawn, pawn2))
		{
			thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn2, desperate: false, out var foodDef, FoodPreferability.RawTasty, allowPlant: false, allowDrug: false, allowCorpse: false, allowDispenserFull: false, allowDispenserEmpty: false, allowForbidden: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap: false, ignoreReservations: false, calculateWantedStackCount: false, FoodPreferability.Undefined, JobDriver_InteractAnimal.RequiredNutritionPerFeed(pawn2) * 2f * 4f);
			if (thing == null)
			{
				JobFailReason.Is("NoFood".Translate());
				return null;
			}
			float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(pawn2) * 2f * 4f;
			float nutrition = FoodUtility.GetNutrition(pawn2, thing, foodDef);
			count = Mathf.CeilToInt(num / nutrition);
		}
		Job job = JobMaker.MakeJob(JobDefOf.Tame, t, null, thing);
		job.count = count;
		return job;
	}
}
