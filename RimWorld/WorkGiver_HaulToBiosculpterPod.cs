using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HaulToBiosculpterPod : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.BiosculpterPod);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckIdeology("Biosculpting"))
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
		CompBiosculpterPod compBiosculpterPod = t.TryGetComp<CompBiosculpterPod>();
		if (compBiosculpterPod == null || !compBiosculpterPod.PowerOn || compBiosculpterPod.State != BiosculpterPodState.LoadingNutrition || (!forced && !compBiosculpterPod.autoLoadNutrition))
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (compBiosculpterPod.RequiredNutritionRemaining > 0f)
		{
			if (FindNutrition(pawn, compBiosculpterPod).Thing == null)
			{
				JobFailReason.Is("NoFood".Translate());
				return false;
			}
			return true;
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompBiosculpterPod compBiosculpterPod = t.TryGetComp<CompBiosculpterPod>();
		if (compBiosculpterPod == null)
		{
			return null;
		}
		if (compBiosculpterPod.RequiredNutritionRemaining > 0f)
		{
			ThingCount thingCount = FindNutrition(pawn, compBiosculpterPod);
			if (thingCount.Thing != null)
			{
				Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
				job.count = Mathf.Min(job.count, thingCount.Count);
				return job;
			}
		}
		return null;
	}

	private ThingCount FindNutrition(Pawn pawn, CompBiosculpterPod pod)
	{
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
		if (thing == null)
		{
			return default(ThingCount);
		}
		int b = Mathf.CeilToInt(pod.RequiredNutritionRemaining / thing.GetStatValue(StatDefOf.Nutrition));
		return new ThingCount(thing, Mathf.Min(thing.stackCount, b));
		bool Validator(Thing x)
		{
			if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
			{
				return false;
			}
			if (!pod.CanAcceptNutrition(x))
			{
				return false;
			}
			return true;
		}
	}
}
