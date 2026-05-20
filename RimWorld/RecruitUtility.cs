using Verse;

namespace RimWorld;

public static class RecruitUtility
{
	public static void Recruit(Pawn pawn, Faction faction, Pawn recruiter = null)
	{
		pawn.apparel?.UnlockAll();
		if (pawn.royalty != null)
		{
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
			{
				if (item.def.replaceOnRecruited != null)
				{
					pawn.royalty.SetTitle(item.faction, item.def.replaceOnRecruited, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
				}
			}
		}
		if (pawn.guest != null)
		{
			pawn.guest.SetGuestStatus(null);
		}
		if (pawn.Faction != faction)
		{
			pawn.SetFaction(faction, recruiter);
		}
		if (pawn.guest != null)
		{
			pawn.guest.Notify_PawnRecruited();
		}
	}
}
