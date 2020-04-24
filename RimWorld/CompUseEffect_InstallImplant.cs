using Verse;

namespace RimWorld
{
	public class CompUseEffect_InstallImplant : CompUseEffect
	{
		public CompProperties_UseEffectInstallImplant Props => (CompProperties_UseEffectInstallImplant)props;

		public override void DoEffect(Pawn user)
		{
			BodyPartRecord bodyPartRecord = user.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback();
			if (bodyPartRecord != null)
			{
				Hediff firstHediffOfDef = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
				if (firstHediffOfDef == null)
				{
					user.health.AddHediff(Props.hediffDef, bodyPartRecord);
				}
				else if (Props.canUpgrade)
				{
					((Hediff_ImplantWithLevel)firstHediffOfDef).ChangeLevel(1);
				}
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if ((!p.IsFreeColonist || p.HasExtraHomeFaction()) && !Props.allowNonColonists)
			{
				failReason = "InstallImplantNotAllowedForNonColonists".Translate();
				return false;
			}
			if (p.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback() == null)
			{
				failReason = "InstallImplantNoBodyPart".Translate() + ": " + Props.bodyPart.LabelShort;
				return false;
			}
			Hediff existingImplant = GetExistingImplant(p);
			if (existingImplant != null)
			{
				if (!Props.canUpgrade)
				{
					failReason = "InstallImplantAlreadyInstalled".Translate();
					return false;
				}
				Hediff_ImplantWithLevel hediff_ImplantWithLevel = (Hediff_ImplantWithLevel)existingImplant;
				if ((float)hediff_ImplantWithLevel.level >= hediff_ImplantWithLevel.def.maxSeverity)
				{
					failReason = "InstallImplantAlreadyMaxLevel".Translate();
					return false;
				}
			}
			failReason = null;
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
}
