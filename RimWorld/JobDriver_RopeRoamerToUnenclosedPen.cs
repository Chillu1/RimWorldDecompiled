using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RopeRoamerToUnenclosedPen : JobDriver_RopeToPen
	{
		protected override bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
		{
			return roperWaitingAtDest;
		}

		protected override void ProcessArrivedRopee(Pawn ropee)
		{
			RoamingMentalState(ropee)?.RecoverFromState();
		}

		protected override bool ShouldOpportunisticallyRopeAnimal(Pawn animal, CompAnimalPenMarker targetPenMarker)
		{
			if (targetPenMarker == base.DestinationMarker)
			{
				return RoamingMentalState(animal) != null;
			}
			return false;
		}

		private static MentalState_Roaming RoamingMentalState(Pawn ropee)
		{
			return ropee.MentalState as MentalState_Roaming;
		}
	}
}
