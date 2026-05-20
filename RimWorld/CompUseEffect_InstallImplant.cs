using Verse;

namespace RimWorld;

public class CompUseEffect_InstallImplant : CompUseEffect
{
	public CompProperties_UseEffectInstallImplant Props => (CompProperties_UseEffectInstallImplant)props;

	public override void DoEffect(Pawn user)
	{
		BodyPartRecord bodyPartRecord = user.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback();
		if (bodyPartRecord != null)
		{
			Hediff firstHediffOfDef = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
			if (firstHediffOfDef == null && !Props.requiresExistingHediff)
			{
				user.health.AddHediff(Props.hediffDef, bodyPartRecord);
			}
			else if (Props.canUpgrade)
			{
				((Hediff_Level)firstHediffOfDef).ChangeLevel(1);
			}
		}
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if ((!p.IsFreeColonist || p.HasExtraHomeFaction()) && !Props.allowNonColonists)
		{
			return "InstallImplantNotAllowedForNonColonists".Translate();
		}
		if (p.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback() == null)
		{
			return "InstallImplantNoBodyPart".Translate() + ": " + Props.bodyPart.LabelShort;
		}
		if (Props.requiresPsychicallySensitive && p.psychicEntropy != null && !p.psychicEntropy.IsPsychicallySensitive)
		{
			return "InstallImplantPsychicallyDeaf".Translate();
		}
		Hediff existingImplant = GetExistingImplant(p);
		if (Props.requiresExistingHediff && existingImplant == null)
		{
			return "InstallImplantHediffRequired".Translate(Props.hediffDef.label);
		}
		if (existingImplant != null)
		{
			if (!Props.canUpgrade)
			{
				return "InstallImplantAlreadyInstalled".Translate();
			}
			Hediff_Level hediff_Level = (Hediff_Level)existingImplant;
			if ((float)hediff_Level.level >= hediff_Level.def.maxSeverity)
			{
				return "InstallImplantAlreadyMaxLevel".Translate();
			}
			if (Props.maxSeverity <= (float)hediff_Level.level)
			{
				return string.Concat("InstallImplantAlreadyMaxLevel".Translate() + " ", Props.maxSeverity.ToString());
			}
			if (Props.minSeverity > (float)hediff_Level.level)
			{
				return "InstallImplantMinLevel".Translate(Props.minSeverity);
			}
		}
		return true;
	}

	public Hediff GetExistingImplant(Pawn p)
	{
		for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
		{
			Hediff hediff = p.health.hediffSet.hediffs[i];
			if (hediff.def == Props.hediffDef && hediff.Part == p.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback())
			{
				return hediff;
			}
		}
		return null;
	}
}
