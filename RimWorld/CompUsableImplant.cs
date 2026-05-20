using Verse;

namespace RimWorld;

public class CompUsableImplant : CompUsable
{
	protected override string FloatMenuOptionLabel(Pawn pawn)
	{
		CompUseEffect_InstallImplant compUseEffect_InstallImplant = parent.TryGetComp<CompUseEffect_InstallImplant>();
		if (compUseEffect_InstallImplant != null && compUseEffect_InstallImplant.GetExistingImplant(pawn) is Hediff_Level hediff_Level && compUseEffect_InstallImplant.Props.canUpgrade && (float)hediff_Level.level < hediff_Level.def.maxSeverity)
		{
			return "UpgradeImplant".Translate(hediff_Level.def.label, hediff_Level.level + 1);
		}
		return base.FloatMenuOptionLabel(pawn);
	}

	public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
	{
		CompUseEffect_InstallImplant useEffectImplant = parent.TryGetComp<CompUseEffect_InstallImplant>();
		Hediff_Level hediff_Level = useEffectImplant.GetExistingImplant(pawn) as Hediff_Level;
		TaggedString text = CompRoyalImplant.CheckForViolations(pawn, useEffectImplant.Props.hediffDef, (hediff_Level != null && useEffectImplant.Props.canUpgrade) ? 1 : 0);
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
			Messages.Message("MessagePsylinkNoSensitivity".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
	}
}
