using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_CollectAnimals : JobDriver_RopeToDestination
	{
		protected override bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
		{
			return false;
		}

		protected override void ProcessArrivedRopee(Pawn ropee)
		{
		}

		protected override bool ShouldOpportunisticallyRopeAnimal(Pawn animal)
		{
			return JobGiver_PrepareCaravan_CollectPawns.DoesAnimalNeedGathering(pawn, animal);
		}

		protected override Thing FindDistantAnimalToRope()
		{
			Lord lord = pawn.GetLord();
			if (lord == null)
			{
				return null;
			}
			return GenClosest.ClosestThing_Global(pawn.Position, lord.ownedPawns, 99999f, (Thing t) => ShouldOpportunisticallyRopeAnimal(t as Pawn));
		}
	}
}
