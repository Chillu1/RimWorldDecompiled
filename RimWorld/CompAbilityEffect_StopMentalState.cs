using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_StopMentalState : CompAbilityEffect
	{
		public new CompProperties_AbilityStopMentalState Props => (CompProperties_AbilityStopMentalState)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			target.Pawn?.MentalState.RecoverFromState();
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
				if (!AbilityUtility.ValidateHasMentalState(pawn, throwMessages))
				{
					return false;
				}
				if (Props.exceptions.Contains(pawn.MentalStateDef))
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
						TaggedString value = ("MentalBreakIntensity" + TargetMentalBreakIntensity(target)).Translate();
						Messages.Message("CommandPsycastNotEnoughPsyfocusForMentalBreak".Translate(num.ToStringPercent(), value, pawn2.psychicEntropy.CurrentPsyfocus.ToStringPercent("0.#"), parent.def.label.Named("PSYCASTNAME"), pawn2.Named("CASTERNAME")), pawn, MessageTypeDefOf.RejectInput, historical: false);
					}
					return false;
				}
			}
			return true;
		}

		public MentalBreakIntensity TargetMentalBreakIntensity(LocalTargetInfo target)
		{
			MentalStateDef mentalStateDef = target.Pawn?.MentalStateDef;
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

		public override string ExtraLabel(LocalTargetInfo target)
		{
			if (target.Pawn != null && Valid(target))
			{
				return "AbilityPsyfocusCost".Translate() + ": " + PsyfocusCostForTarget(target).ToStringPercent("0.#");
			}
			return null;
		}
	}
}
