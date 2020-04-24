using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_TakeDrugsForDrugPolicy : ThinkNode_JobGiver
	{
		public override float GetPriority(Pawn pawn)
		{
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug))
				{
					return 7.5f;
				}
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug))
				{
					Thing thing = FindDrugFor(pawn, currentPolicy[i].drug);
					if (thing != null)
					{
						return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
					}
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
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(drugDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, x));
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
}
