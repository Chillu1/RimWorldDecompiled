using Verse;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherAnimals : JobDriver_RopeToDestination
	{
		protected override bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
		{
			IntVec3 intVec = pawn.mindState.duty?.focus.Cell ?? IntVec3.Invalid;
			if (!intVec.IsValid)
			{
				return false;
			}
			if (!pawn.Position.InHorDistOf(intVec, 2f))
			{
				return false;
			}
			District district = intVec.GetDistrict(pawn.Map);
			if (district != pawn.GetDistrict() || district != ropee.GetDistrict())
			{
				return false;
			}
			return true;
		}

		protected override void ProcessArrivedRopee(Pawn ropee)
		{
			IntVec3 spot = ropee.mindState.duty?.focus.Cell ?? IntVec3.Invalid;
			if (spot.IsValid)
			{
				ropee.roping.RopeToSpot(spot);
			}
		}

		protected override bool ShouldOpportunisticallyRopeAnimal(Pawn animal)
		{
			return JobGiver_PrepareCaravan_GatherPawns.DoesAnimalNeedGathering(pawn, animal);
		}
	}
}
