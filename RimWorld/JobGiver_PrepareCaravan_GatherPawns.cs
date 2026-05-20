using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_PrepareCaravan_GatherPawns : JobGiver_PrepareCaravan_RopePawns
{
	protected override JobDef RopeJobDef => JobDefOf.PrepareCaravan_GatherAnimals;

	protected override bool AnimalNeedsGathering(Pawn pawn, Pawn animal)
	{
		return DoesAnimalNeedGathering(pawn, animal);
	}

	public static bool DoesAnimalNeedGathering(Pawn pawn, Pawn animal)
	{
		IntVec3 cell = pawn.mindState.duty.focus.Cell;
		if (AnimalPenUtility.NeedsToBeManagedByRope(animal) && !animal.roping.IsRopedByPawn && (!animal.roping.IsRopedToSpot || !(animal.roping.RopedToSpot == cell)) && pawn.GetLord() == animal.GetLord() && GatherAnimalsAndSlavesForCaravanUtility.CanRoperTakeAnimalToDest(pawn, animal, cell))
		{
			return pawn.CanReserve(animal);
		}
		return false;
	}
}
