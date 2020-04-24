using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class UnloadCarriersJobGiverUtility
	{
		public static bool HasJobOnThing(Pawn pawn, Thing t, bool forced)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || pawn2 == pawn || pawn2.IsFreeColonist || !pawn2.inventory.UnloadEverything || (pawn2.Faction != pawn.Faction && pawn2.HostFaction != pawn.Faction) || t.IsForbidden(pawn) || t.IsBurning() || !pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			return true;
		}
	}
}
