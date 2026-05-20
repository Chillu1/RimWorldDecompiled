using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_MoveDrugsToInventory : ThinkNode_JobGiver
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
			if (pawn.drugs.AllowedToTakeToInventory(drugPolicy[i].drug))
			{
				return 7.5f;
			}
		}
		return 0f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		DrugPolicy drugPolicy = pawn.drugs?.CurrentPolicy;
		if (drugPolicy == null)
		{
			return null;
		}
		for (int i = 0; i < drugPolicy.Count; i++)
		{
			if (pawn.drugs.AllowedToTakeToInventory(drugPolicy[i].drug))
			{
				int num = drugPolicy[i].takeToInventory - pawn.inventory.innerContainer.TotalStackCountOfDef(drugPolicy[i].drug);
				Thing thing = FindDrugFor(pawn, drugPolicy[i].drug, num);
				if (thing != null)
				{
					Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
					job.count = Mathf.Min(num, thing.stackCount);
					return job;
				}
			}
		}
		return null;
	}

	private Thing FindDrugFor(Pawn pawn, ThingDef drugDef, int desired)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(drugDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, x, desired));
	}

	private bool DrugValidator(Pawn pawn, Thing drug, int desired)
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
			if (!pawn.CanReserve(drug, 10, Mathf.Min(drug.stackCount, desired)))
			{
				return false;
			}
		}
		return true;
	}
}
