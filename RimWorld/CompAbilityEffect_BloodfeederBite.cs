using System;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_BloodfeederBite : CompAbilityEffect
{
	public new CompProperties_AbilityBloodfeederBite Props => (CompProperties_AbilityBloodfeederBite)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			SanguophageUtility.DoBite(parent.pawn, pawn, Props.hemogenGain, Props.nutritionGain, Props.targetBloodLoss, Props.resistanceGain, Props.bloodFilthToSpawnRange, Props.thoughtDefToGiveTarget, Props.opinionThoughtDefToGiveTarget);
		}
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return Valid(target);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return false;
		}
		if (!AbilityUtility.ValidateMustBeHumanOrWildMan(pawn, throwMessages, parent))
		{
			return false;
		}
		if (pawn.Faction != null && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
		{
			if (pawn.Faction.HostileTo(parent.pawn.Faction))
			{
				if (!pawn.Downed)
				{
					if (throwMessages)
					{
						Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
					}
					return false;
				}
			}
			else if (pawn.IsQuestLodger() || pawn.Faction != parent.pawn.Faction)
			{
				if (throwMessages)
				{
					Messages.Message("MessageCannotUseOnOtherFactions".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
		}
		if (pawn.IsWildMan() && !pawn.IsPrisonerOfColony && !pawn.Downed)
		{
			if (throwMessages)
			{
				Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (pawn.InMentalState || PrisonBreakUtility.IsPrisonBreaking(pawn))
		{
			if (throwMessages)
			{
				Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (ModsConfig.AnomalyActive && pawn.IsMutant && !pawn.mutant.Def.canBleed)
		{
			if (throwMessages)
			{
				Messages.Message("MessageCannotUseOnNonBleeder".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			string text = null;
			if (pawn.HostileTo(parent.pawn) && !pawn.Downed)
			{
				text += "MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY"));
			}
			float num = BloodlossAfterBite(pawn);
			if (num >= HediffDefOf.BloodLoss.lethalSeverity)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "WillKill".Translate();
			}
			else if (HediffDefOf.BloodLoss.stages[HediffDefOf.BloodLoss.StageAtSeverity(num)].lifeThreatening)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "WillCauseSeriousBloodloss".Translate();
			}
			return text;
		}
		return base.ExtraLabelMouseAttachment(target);
	}

	public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless))
			{
				return null;
			}
			float num = BloodlossAfterBite(pawn);
			if (num >= HediffDefOf.BloodLoss.lethalSeverity)
			{
				return Dialog_MessageBox.CreateConfirmation("WarningPawnWillDieFromBloodfeeding".Translate(pawn.Named("PAWN")), confirmAction, destructive: true);
			}
			if (HediffDefOf.BloodLoss.stages[HediffDefOf.BloodLoss.StageAtSeverity(num)].lifeThreatening)
			{
				return Dialog_MessageBox.CreateConfirmation("WarningPawnWillHaveSeriousBloodlossFromBloodfeeding".Translate(pawn.Named("PAWN")), confirmAction, destructive: true);
			}
		}
		return null;
	}

	private float BloodlossAfterBite(Pawn target)
	{
		if (target.Dead || !target.RaceProps.IsFlesh)
		{
			return 0f;
		}
		float num = Props.targetBloodLoss;
		Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
		if (firstHediffOfDef != null)
		{
			num += firstHediffOfDef.Severity;
		}
		return num;
	}
}
