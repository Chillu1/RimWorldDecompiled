using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SpectateInCircleDuty : JobGiver_SpectateDutySpectateRect
	{
		protected override bool TryFindSpot(Pawn pawn, PawnDuty duty, out IntVec3 spot)
		{
			if (!SpectatorCellFinder.TryFindCircleSpectatorCellFor(pawn, duty.spectateRect, duty.spectateDistance.min, duty.spectateDistance.max, pawn.Map, out spot, null, RitualUtility.GoodSpectateCellForRitual))
			{
				return base.TryFindSpot(pawn, duty, out spot);
			}
			return spot.IsValid;
		}
	}
}
