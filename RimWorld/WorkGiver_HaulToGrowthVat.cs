using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HaulToGrowthVat : WorkGiver_Scanner
{
	private const float NutritionBuffer = 2.5f;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.GrowthVat);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Growth vat"))
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (!(t is Building_GrowthVat building_GrowthVat))
		{
			return false;
		}
		if (building_GrowthVat.NutritionNeeded > 2.5f)
		{
			if (FindNutrition(pawn, building_GrowthVat).Thing == null)
			{
				JobFailReason.Is("NoFood".Translate());
				return false;
			}
			return true;
		}
		if (building_GrowthVat.selectedEmbryo != null && !building_GrowthVat.innerContainer.Contains(building_GrowthVat.selectedEmbryo))
		{
			return CanHaulSelectedThing(pawn, building_GrowthVat.selectedEmbryo);
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building_GrowthVat building_GrowthVat))
		{
			return null;
		}
		if (building_GrowthVat.NutritionNeeded > 0f)
		{
			ThingCount thingCount = FindNutrition(pawn, building_GrowthVat);
			if (thingCount.Thing != null)
			{
				Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
				job.count = Mathf.Min(job.count, thingCount.Count);
				return job;
			}
		}
		if (building_GrowthVat.selectedEmbryo != null && !building_GrowthVat.innerContainer.Contains(building_GrowthVat.selectedEmbryo) && CanHaulSelectedThing(pawn, building_GrowthVat.selectedEmbryo))
		{
			Job job2 = HaulAIUtility.HaulToContainerJob(pawn, building_GrowthVat.selectedEmbryo, t);
			job2.count = 1;
			return job2;
		}
		return null;
	}

	private bool CanHaulSelectedThing(Pawn pawn, Thing selectedThing)
	{
		if (!selectedThing.Spawned || selectedThing.Map != pawn.Map)
		{
			return false;
		}
		if (selectedThing.IsForbidden(pawn) || !pawn.CanReserveAndReach(selectedThing, PathEndMode.OnCell, Danger.Deadly, 1, 1))
		{
			return false;
		}
		return true;
	}

	private ThingCount FindNutrition(Pawn pawn, Building_GrowthVat vat)
	{
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
		if (thing == null)
		{
			return default(ThingCount);
		}
		int b = Mathf.CeilToInt(vat.NutritionNeeded / thing.GetStatValue(StatDefOf.Nutrition));
		return new ThingCount(thing, Mathf.Min(thing.stackCount, b));
		bool Validator(Thing x)
		{
			if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
			{
				return false;
			}
			if (!vat.CanAcceptNutrition(x))
			{
				return false;
			}
			if (x.def.GetStatValueAbstract(StatDefOf.Nutrition) > vat.NutritionNeeded)
			{
				return false;
			}
			return true;
		}
	}
}
