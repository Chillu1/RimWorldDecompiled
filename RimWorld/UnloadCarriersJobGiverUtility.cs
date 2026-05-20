using Verse;
using Verse.AI;

namespace RimWorld;

public static class UnloadCarriersJobGiverUtility
{
	public static bool HasJobOnThing(Pawn pawn, Thing t, bool forced)
	{
		if (!(t is Pawn pawn2) || pawn2 == pawn || pawn2.IsFreeColonist || !pawn2.inventory.UnloadEverything || (pawn2.Faction != pawn.Faction && pawn2.HostFaction != pawn.Faction) || t.IsForbidden(pawn) || t.IsBurning() || !pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}
}
