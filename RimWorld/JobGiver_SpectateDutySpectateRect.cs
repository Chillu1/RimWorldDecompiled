using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_SpectateDutySpectateRect : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		PawnDuty duty = pawn.mindState.duty;
		if (duty == null)
		{
			return null;
		}
		if (!TryFindSpot(pawn, duty, out var spot))
		{
			return null;
		}
		IntVec3 centerCell = duty.spectateRect.CenterCell;
		Building edifice = spot.GetEdifice(pawn.Map);
		if (edifice != null && pawn.CanReserveSittableOrSpot(spot))
		{
			return JobMaker.MakeJob(JobDefOf.SpectateCeremony, spot, centerCell, edifice);
		}
		return JobMaker.MakeJob(JobDefOf.SpectateCeremony, spot, centerCell);
	}

	protected virtual bool TryFindSpot(Pawn pawn, PawnDuty duty, out IntVec3 spot)
	{
		Precept_Ritual ritual = null;
		if (pawn.GetLord() != null && pawn.GetLord().LordJob is LordJob_Ritual lordJob_Ritual)
		{
			ritual = lordJob_Ritual.Ritual;
		}
		if ((duty.spectateRectPreferredSide == SpectateRectSide.None || !SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, pawn.Map, out spot, duty.spectateRectPreferredSide, 1, null, ritual, RitualUtility.GoodSpectateCellForRitual)) && !SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, pawn.Map, out spot, duty.spectateRectAllowedSides, 1, null, ritual, RitualUtility.GoodSpectateCellForRitual))
		{
			IntVec3 target = duty.spectateRect.CenterCell;
			if (CellFinder.TryFindRandomReachableNearbyCell(target, pawn.MapHeld, 5f, TraverseParms.For(pawn), (IntVec3 c) => c.GetRoom(pawn.MapHeld) == target.GetRoom(pawn.MapHeld) && pawn.CanReserveSittableOrSpot(c) && !duty.spectateRect.Contains(c), null, out spot))
			{
				return true;
			}
			Log.Warning("Failed to find a spectator spot for " + pawn);
			return false;
		}
		return true;
	}
}
