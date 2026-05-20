using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompRoyalImplant : ThingComp
{
	public CompProperties_RoyalImplant Props => (CompProperties_RoyalImplant)props;

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		Pair<Faction, RoyalTitleDef> minTitleForImplantAllFactions = Faction.GetMinTitleForImplantAllFactions(Props.implantHediff);
		if (minTitleForImplantAllFactions.First == null)
		{
			yield break;
		}
		Faction first = minTitleForImplantAllFactions.First;
		StringBuilder stringBuilder = new StringBuilder("Stat_Thing_MinimumRoyalTitle_Desc".Translate(first.Named("FACTION")));
		if (typeof(Hediff_Level).IsAssignableFrom(Props.implantHediff.hediffClass))
		{
			stringBuilder.Append("\n\n" + "Stat_Thing_MinimumRoyalTitle_Level_Desc".Translate(first.Named("FACTION")) + ":\n\n");
			for (int i = 1; (float)i <= Props.implantHediff.maxSeverity; i++)
			{
				stringBuilder.Append(" -  x" + i + ", " + first.GetMinTitleForImplant(Props.implantHediff, i).GetLabelCapForBothGenders() + "\n");
			}
		}
		yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "Stat_Thing_MinimumRoyalTitle_Name".Translate(first.Named("FACTION")).Resolve(), minTitleForImplantAllFactions.Second.GetLabelCapForBothGenders(), stringBuilder.ToTaggedString().Resolve(), 2100);
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Pair<Faction, RoyalTitleDef> minTitleForImplantAllFactions = Faction.GetMinTitleForImplantAllFactions(Props.implantHediff);
		if (minTitleForImplantAllFactions.First != null)
		{
			stringBuilder.AppendLine("MinimumRoyalTitleInspectString".Translate(minTitleForImplantAllFactions.First.Named("FACTION"), minTitleForImplantAllFactions.Second.GetLabelCapForBothGenders().Named("TITLE")).Resolve());
		}
		if (stringBuilder.Length <= 0)
		{
			return null;
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public static TaggedString CheckForViolations(Pawn pawn, HediffDef hediff, int levelOffset)
	{
		if (levelOffset < 0)
		{
			return "";
		}
		if (pawn.Faction != Faction.OfPlayer || !hediff.HasComp(typeof(HediffComp_RoyalImplant)))
		{
			return "";
		}
		Hediff_Level hediff_Level = (Hediff_Level)pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == hediff);
		int num = ((levelOffset != 0 && hediff_Level != null) ? (hediff_Level.level + levelOffset) : 0);
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			if (pawn.Faction != null && !item.Hidden && !item.HostileTo(Faction.OfPlayer) && ThingRequiringRoyalPermissionUtility.IsViolatingRulesOf(hediff, pawn, item, num))
			{
				RoyalTitleDef minTitleForImplant = item.GetMinTitleForImplant(hediff, num);
				HediffCompProperties_RoyalImplant hediffCompProperties_RoyalImplant = hediff.CompProps<HediffCompProperties_RoyalImplant>();
				string arg = hediff.label + ((num == 0) ? "" : (" (" + num + "x)"));
				TaggedString taggedString = hediffCompProperties_RoyalImplant.violationTriggerDescriptionKey.Translate(pawn.Named("PAWN"));
				TaggedString taggedString2 = "RoyalImplantIllegalUseWarning".Translate(pawn.Named("PAWN"), arg.Named("IMPLANT"), item.Named("FACTION"), minTitleForImplant.GetLabelCapFor(pawn).Named("TITLE"), taggedString.Named("VIOLATIONTRIGGER"));
				if (levelOffset != 0)
				{
					return taggedString2 + ("\n\n" + "RoyalImplantUpgradeConfirmation".Translate());
				}
				return taggedString2 + ("\n\n" + "RoyalImplantInstallConfirmation".Translate());
			}
		}
		return "";
	}
}
