using Verse;

namespace RimWorld
{
	public class CompUsableImplant : CompUsable
	{
		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			CompUseEffect_InstallImplant compUseEffect_InstallImplant = parent.TryGetComp<CompUseEffect_InstallImplant>();
			if (compUseEffect_InstallImplant != null)
			{
				Hediff_ImplantWithLevel hediff_ImplantWithLevel = compUseEffect_InstallImplant.GetExistingImplant(pawn) as Hediff_ImplantWithLevel;
				if (hediff_ImplantWithLevel != null && compUseEffect_InstallImplant.Props.canUpgrade && (float)hediff_ImplantWithLevel.level < hediff_ImplantWithLevel.def.maxSeverity)
				{
					return "UpgradeImplant".Translate(parent.LabelShort, hediff_ImplantWithLevel.level + 1);
				}
			}
			return base.FloatMenuOptionLabel(pawn);
		}

		public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget)
		{
			CompUseEffect_InstallImplant useEffectImplant = parent.TryGetComp<CompUseEffect_InstallImplant>();
			Hediff_ImplantWithLevel hediff_ImplantWithLevel = useEffectImplant.GetExistingImplant(pawn) as Hediff_ImplantWithLevel;
			TaggedString text = CompRoyalImplant.CheckForViolations(pawn, useEffectImplant.Props.hediffDef, (hediff_ImplantWithLevel != null && useEffectImplant.Props.canUpgrade) ? 1 : 0);
			if (!text.NullOrEmpty())
			{
				Find.WindowStack.Add(new Dialog_MessageBox(text, "Yes".Translate(), delegate
				{
					UseJobInternal(pawn, extraTarget, useEffectImplant.Props.hediffDef);
				}, "No".Translate()));
			}
			else
			{
				UseJobInternal(pawn, extraTarget, useEffectImplant.Props.hediffDef);
			}
		}

		private void UseJobInternal(Pawn pawn, LocalTargetInfo extraTarget, HediffDef hediff)
		{
			base.TryStartUseJob(pawn, extraTarget);
			if (hediff == HediffDefOf.PsychicAmplifier && pawn.GetStatValue(StatDefOf.PsychicSensitivity) < float.Epsilon)
			{
				Messages.Message("MessagePsychicAmplifierNoSensitivity".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
	}
}
