using Verse;

namespace RimWorld;

public static class RoyalTitleDefExt
{
	public static RoyalTitleDef GetNextTitle(this RoyalTitleDef currentTitle, Faction faction)
	{
		int num = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle);
		if (num == -1 && currentTitle != null)
		{
			return null;
		}
		int num2 = ((currentTitle != null) ? (num + 1) : 0);
		if (faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count <= num2)
		{
			return null;
		}
		return faction.def.RoyalTitlesAwardableInSeniorityOrderForReading[num2];
	}

	public static RoyalTitleDef GetPreviousTitle(this RoyalTitleDef currentTitle, Faction faction)
	{
		if (currentTitle == null)
		{
			return null;
		}
		int num = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) - 1;
		if (num >= faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count || num < 0)
		{
			return null;
		}
		return faction.def.RoyalTitlesAwardableInSeniorityOrderForReading[num];
	}

	public static RoyalTitleDef GetPreviousTitle_IncludeNonRewardable(this RoyalTitleDef currentTitle, Faction faction)
	{
		if (currentTitle == null)
		{
			return null;
		}
		int num = faction.def.RoyalTitlesAllInSeniorityOrderForReading.IndexOf(currentTitle) - 1;
		if (num >= faction.def.RoyalTitlesAllInSeniorityOrderForReading.Count || num < 0)
		{
			return null;
		}
		return faction.def.RoyalTitlesAllInSeniorityOrderForReading[num];
	}

	public static bool TryInherit(this RoyalTitleDef title, Pawn from, Faction faction, out RoyalTitleInheritanceOutcome outcome)
	{
		outcome = default(RoyalTitleInheritanceOutcome);
		if (title.GetInheritanceWorker(faction) == null)
		{
			return false;
		}
		Pawn heir = from.royalty.GetHeir(faction);
		if (heir == null || heir.Destroyed)
		{
			return false;
		}
		RoyalTitleDef currentTitle = heir.royalty.GetCurrentTitle(faction);
		bool heirTitleHigher = currentTitle != null && currentTitle.seniority >= title.seniority;
		outcome = new RoyalTitleInheritanceOutcome
		{
			heir = heir,
			heirCurrentTitle = currentTitle,
			heirTitleHigher = heirTitleHigher
		};
		return true;
	}
}
