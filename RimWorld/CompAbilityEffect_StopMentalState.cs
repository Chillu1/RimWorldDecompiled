using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_StopMentalState : CompAbilityEffect
{
	public new CompProperties_AbilityStopMentalState Props => (CompProperties_AbilityStopMentalState)props;

	public override bool HideTargetPawnTooltip => true;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.CatatonicBreakdown);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
		pawn?.MentalState?.RecoverFromState();
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return Valid(target);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (!AbilityUtility.ValidateHasMentalState(pawn, throwMessages, parent))
			{
				return false;
			}
			if (pawn.MentalStateDef != null && Props.exceptions.Contains(pawn.MentalStateDef))
			{
				if (throwMessages)
				{
					Messages.Message("AbilityDoesntWorkOnMentalState".Translate(parent.def.label, pawn.MentalStateDef.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			float num = PsyfocusCostForTarget(target);
			if (num > parent.pawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
			{
				Pawn pawn2 = parent.pawn;
				if (throwMessages)
				{
					TaggedString taggedString = ("MentalBreakIntensity" + TargetMentalBreakIntensity(target)).Translate();
					Messages.Message("CommandPsycastNotEnoughPsyfocusForMentalBreak".Translate(num.ToStringPercent(), taggedString, pawn2.psychicEntropy.CurrentPsyfocus.ToStringPercent("0.#"), parent.def.label.Named("PSYCASTNAME"), pawn2.Named("CASTERNAME")), pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
		}
		return true;
	}

	public MentalBreakIntensity TargetMentalBreakIntensity(LocalTargetInfo target)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			MentalStateDef mentalStateDef = pawn.MentalStateDef;
			if (mentalStateDef != null)
			{
				List<MentalBreakDef> allDefsListForReading = DefDatabase<MentalBreakDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].mentalState == mentalStateDef)
					{
						return allDefsListForReading[i].intensity;
					}
				}
			}
			else if (pawn.health.hediffSet.HasHediff(HediffDefOf.CatatonicBreakdown))
			{
				return MentalBreakIntensity.Extreme;
			}
		}
		return MentalBreakIntensity.Minor;
	}

	public override float PsyfocusCostForTarget(LocalTargetInfo target)
	{
		return TargetMentalBreakIntensity(target) switch
		{
			MentalBreakIntensity.Minor => Props.psyfocusCostForMinor, 
			MentalBreakIntensity.Major => Props.psyfocusCostForMajor, 
			MentalBreakIntensity.Extreme => Props.psyfocusCostForExtreme, 
			_ => 0f, 
		};
	}

	public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		if (target.Pawn != null && Valid(target))
		{
			return "AbilityPsyfocusCost".Translate() + ": " + PsyfocusCostForTarget(target).ToStringPercent("0.#");
		}
		return null;
	}
}
