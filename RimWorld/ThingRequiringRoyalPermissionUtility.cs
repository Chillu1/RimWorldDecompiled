using System;
using Verse;

namespace RimWorld
{
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

		[Obsolete("Only need this overload to not break mod compatibility.")]
		public static bool IsViolatingRulesOfAnyFaction(Def implantOrWeapon, Pawn pawn, int implantLevel = 0)
		{
			return IsViolatingRulesOfAnyFaction_NewTemp(implantOrWeapon, pawn, implantLevel);
		}

		public static bool IsViolatingRulesOfAnyFaction_NewTemp(Def implantOrWeapon, Pawn pawn, int implantLevel = 0, bool ignoreSilencer = false)
		{
			if (!ignoreSilencer && pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicSilencer))
			{
				return false;
			}
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
			HediffDef implantDef;
			if ((implantDef = (implantOrWeapon as HediffDef)) != null)
			{
				return faction.GetMinTitleForImplant(implantDef, implantLevel);
			}
			ThingDef thingDef;
			if ((thingDef = (implantOrWeapon as ThingDef)) != null && thingDef.HasComp(typeof(CompBladelinkWeapon)))
			{
				return faction.def.minTitleForBladelinkWeapons;
			}
			return null;
		}

		public static TaggedString GetEquipWeaponConfirmationDialogText(Thing weapon, Pawn pawn)
		{
			if (pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicSilencer))
			{
				return null;
			}
			foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
			{
				if (pawn.Faction != null && !item.def.hidden && !item.HostileTo(Faction.OfPlayer) && IsViolatingRulesOf(weapon.def, pawn, item))
				{
					RoyalTitleDef minTitleToUse = GetMinTitleToUse(weapon.def, item);
					return "RoyalWeaponIllegalUseWarning".Translate(pawn.Named("PAWN"), weapon.Named("WEAPON"), item.Named("FACTION"), minTitleToUse.GetLabelCapFor(pawn).Named("TITLE"));
				}
			}
			return null;
		}
	}
}
