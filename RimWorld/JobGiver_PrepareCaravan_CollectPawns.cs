using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_PrepareCaravan_CollectPawns : JobGiver_PrepareCaravan_RopePawns
{
	protected override JobDef RopeJobDef => JobDefOf.PrepareCaravan_CollectAnimals;

	protected override bool AnimalNeedsGathering(Pawn pawn, Pawn animal)
	{
		return DoesAnimalNeedGathering(pawn, animal);
	}

	public static bool DoesAnimalNeedGathering(Pawn pawn, Pawn animal)
	{
		IntVec3 cell = pawn.mindState.duty.focus.Cell;
		if (AnimalPenUtility.NeedsToBeManagedByRope(animal) && !GatherAnimalsAndSlavesForCaravanUtility.IsRopedByCaravanPawn(animal) && pawn.GetLord() == animal.GetLord() && GatherAnimalsAndSlavesForCaravanUtility.CanRoperTakeAnimalToDest(pawn, animal, cell))
		{
			return pawn.CanReserve(animal);
		}
		return false;
	}
}
