using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class WorkGiver_InteractAnimal : WorkGiver_Scanner
{
	protected static string NoUsableFoodTrans;

	protected static string AnimalInteractedTooRecentlyTrans;

	private static string CantInteractAnimalDownedTrans;

	private static string CantInteractAnimalAsleepTrans;

	private static string CantInteractAnimalBusyTrans;

	protected bool canInteractWhileSleeping;

	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public static void ResetStaticData()
	{
		NoUsableFoodTrans = "NoUsableFood".Translate();
		AnimalInteractedTooRecentlyTrans = "AnimalInteractedTooRecently".Translate();
		CantInteractAnimalDownedTrans = "CantInteractAnimalDowned".Translate();
		CantInteractAnimalAsleepTrans = "CantInteractAnimalAsleep".Translate();
		CantInteractAnimalBusyTrans = "CantInteractAnimalBusy".Translate();
	}

	protected virtual bool CanInteractWithAnimal(Pawn pawn, Pawn animal, bool forced)
	{
		if (CanInteractWithAnimal(pawn, animal, out var jobFailReason, forced, canInteractWhileSleeping))
		{
			return true;
		}
		if (jobFailReason != null)
		{
			JobFailReason.Is(jobFailReason);
		}
		return false;
	}

	public static bool CanInteractWithAnimal(Pawn pawn, Pawn animal, out string jobFailReason, bool forced, bool canInteractWhileSleeping = false, bool ignoreSkillRequirements = false, bool canInteractWhileRoaming = false)
	{
		jobFailReason = null;
		if (!pawn.CanReserve(animal, 1, -1, null, forced))
		{
			return false;
		}
		if (animal.Downed)
		{
			jobFailReason = CantInteractAnimalDownedTrans;
			return false;
		}
		if (!animal.Awake() && !canInteractWhileSleeping)
		{
			jobFailReason = CantInteractAnimalAsleepTrans;
			return false;
		}
		if (!animal.CanCasuallyInteractNow(twoWayInteraction: false, canInteractWhileSleeping, canInteractWhileRoaming))
		{
			jobFailReason = CantInteractAnimalBusyTrans;
			return false;
		}
		int num = TrainableUtility.MinimumHandlingSkill(animal);
		if (!ignoreSkillRequirements && num > pawn.skills.GetSkill(SkillDefOf.Animals).Level)
		{
			jobFailReason = "AnimalsSkillTooLow".Translate(num);
			return false;
		}
		return true;
	}

	protected bool HasFoodToInteractAnimal(Pawn pawn, Pawn tamee)
	{
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		int num = 0;
		float num2 = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);
		float num3 = 0f;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			Thing thing = innerContainer[i];
			if (!tamee.WillEat(thing, pawn) || (int)thing.def.ingestible.preferability > 5 || thing.def.IsDrug)
			{
				continue;
			}
			for (int j = 0; j < thing.stackCount; j++)
			{
				num3 += thing.GetStatValue(StatDefOf.Nutrition);
				if (num3 >= num2)
				{
					num++;
					num3 = 0f;
				}
				if (num >= 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected Job TakeFoodForAnimalInteractJob(Pawn pawn, Pawn tamee)
	{
		ThingDef foodDef;
		Thing thing = FoodUtility.BestFoodSourceOnMap(pawn, tamee, desperate: false, out foodDef, FoodPreferability.RawTasty, allowPlant: false, allowDrug: false, allowCorpse: false, allowDispenserFull: false, allowDispenserEmpty: false, allowForbidden: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap: false, ignoreReservations: false, calculateWantedStackCount: false, FoodPreferability.Undefined, JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f);
		if (thing == null)
		{
			return null;
		}
		float wantedNutrition = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
		float nutrition = FoodUtility.GetNutrition(tamee, thing, foodDef);
		int count = FoodUtility.StackCountForNutrition(wantedNutrition, nutrition);
		Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
		job.count = count;
		return job;
	}
}
