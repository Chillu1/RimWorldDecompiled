using System;
using Verse;

namespace RimWorld;

public static class ThingRequiringRoyalPermissionUtility
{
	public static bool IsViolatingRulesOf(Def implantOrWeapon, Pawn pawn, Faction faction, int implantLevel = 0)
	{
		if (faction.def.royalImplantRules == null || faction.def.royalImplantRules.Count == 0)
		{
			return false;
		}
		RoyalTitleDef minTitleToUse = GetMinTitleToUse(implantOrWeapon, faction, implantLevel);
		if (minTitleToUse == null)
		{
			return false;
		}
		RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(faction);
		if (currentTitle == null)
		{
			return true;
		}
		int num = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle);
		if (num < 0)
		{
			return false;
		}
		int num2 = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(minTitleToUse);
		return num < num2;
	}

	public static bool IsViolatingRulesOfAnyFaction(Def implantOrWeapon, Pawn pawn, int implantLevel = 0)
	{
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (IsViolatingRulesOf(implantOrWeapon, pawn, allFaction, implantLevel))
			{
				return true;
			}
		}
		return false;
	}

	public static RoyalTitleDef GetMinTitleToUse(Def implantOrWeapon, Faction faction, int implantLevel = 0)
	{
		if (implantOrWeapon is HediffDef implantDef)
		{
			return faction.GetMinTitleForImplant(implantDef, implantLevel);
		}
		return null;
	}

	[Obsolete("Will be removed in the future")]
	public static TaggedString GetEquipWeaponConfirmationDialogText(Thing weapon, Pawn pawn)
	{
		return null;
	}
}
