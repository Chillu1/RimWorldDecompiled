using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_TakeDrugsForDrugPolicy : ThinkNode_JobGiver
{
	public override float GetPriority(Pawn pawn)
	{
		DrugPolicy drugPolicy = pawn.drugs?.CurrentPolicy;
		if (drugPolicy == null)
		{
			return 0f;
		}
		for (int i = 0; i < drugPolicy.Count; i++)
		{
			if (pawn.drugs.ShouldTryToTakeScheduledNow(drugPolicy[i].drug))
			{
				return 7.5f;
			}
		}
		return 0f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
		if (currentPolicy == null)
		{
			return null;
		}
		for (int i = 0; i < currentPolicy.Count; i++)
		{
			if (!pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug))
			{
				continue;
			}
			Thing thing = FindDrugFor(pawn, currentPolicy[i].drug);
			if (thing != null)
			{
				Pawn pawn2 = (thing.ParentHolder as Pawn_InventoryTracker)?.pawn;
				if (pawn2 != null && pawn2 != pawn)
				{
					Job job = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, pawn2);
					job.count = 1;
					return job;
				}
				return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
			}
		}
		return null;
	}

	private Thing FindDrugFor(Pawn pawn, ThingDef drugDef)
	{
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (innerContainer[i].def == drugDef && DrugValidator(pawn, innerContainer[i]))
			{
				return innerContainer[i];
			}
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(drugDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, x));
		if (thing != null)
		{
			return thing;
		}
		if (pawn.IsColonist && pawn.Map != null)
		{
			foreach (Pawn spawnedColonyAnimal in pawn.Map.mapPawns.SpawnedColonyAnimals)
			{
				foreach (Thing item in spawnedColonyAnimal.inventory.innerContainer)
				{
					if (item.def == drugDef && DrugValidator(pawn, item) && !spawnedColonyAnimal.IsForbidden(pawn) && pawn.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private bool DrugValidator(Pawn pawn, Thing drug)
	{
		if (!drug.def.IsDrug)
		{
			return false;
		}
		if (drug.Spawned)
		{
			if (drug.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(drug, 10, 1))
			{
				return false;
			}
			if (!drug.IsSociallyProper(pawn))
			{
				return false;
			}
		}
		return true;
	}
}
