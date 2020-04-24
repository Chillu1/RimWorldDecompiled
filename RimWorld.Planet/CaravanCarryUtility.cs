using Verse;

namespace RimWorld.Planet
{
	public static class CaravanCarryUtility
	{
		public static bool CarriedByCaravan(this Pawn p)
		{
			return p.GetCaravan()?.carryTracker.IsCarried(p) ?? false;
		}

		public static bool WouldBenefitFromBeingCarried(Pawn p)
		{
			return CaravanBedUtility.WouldBenefitFromRestingInBed(p);
		}
	}
}
