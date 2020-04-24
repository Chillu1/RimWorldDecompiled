using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PrisonerWaitInsteadOfEscaping : JobGiver_Wander
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.guest == null || !pawn.guest.ShouldWaitInsteadOfEscaping)
			{
				return null;
			}
			Room room = pawn.GetRoom();
			if (room != null && room.isPrisonCell)
			{
				return null;
			}
			IntVec3 result = pawn.guest.spotToWaitInsteadOfEscaping;
			if (!result.IsValid || !pawn.CanReach(result, PathEndMode.OnCell, Danger.Deadly))
			{
				if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out result))
				{
					return null;
				}
				pawn.guest.spotToWaitInsteadOfEscaping = result;
			}
			return base.TryGiveJob(pawn);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.guest.spotToWaitInsteadOfEscaping;
		}
	}
}
