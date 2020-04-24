using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class RestraintsUtility
	{
		public static bool InRestraints(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			if (pawn.HostFaction == null)
			{
				return false;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob != null && lord.LordJob.NeverInRestraints)
			{
				return false;
			}
			if (pawn.guest != null && pawn.guest.Released)
			{
				return false;
			}
			return true;
		}

		public static bool ShouldShowRestraintsInfo(Pawn pawn)
		{
			if (pawn.IsPrisonerOfColony)
			{
				return InRestraints(pawn);
			}
			return false;
		}
	}
}
