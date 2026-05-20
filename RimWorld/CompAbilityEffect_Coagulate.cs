using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Coagulate : CompAbilityEffect
{
	private new CompProperties_AbilityCoagulate Props => (CompProperties_AbilityCoagulate)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return;
		}
		int num = 0;
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int num2 = hediffs.Count - 1; num2 >= 0; num2--)
		{
			if ((hediffs[num2] is Hediff_Injury || hediffs[num2] is Hediff_MissingPart) && hediffs[num2].TendableNow())
			{
				hediffs[num2].Tended(Props.tendQualityRange.RandomInRange, Props.tendQualityRange.TrueMax, 1);
				num++;
			}
		}
		if (num > 0)
		{
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "NumWoundsTended".Translate(num), 3.65f);
		}
		FleckMaker.AttachedOverlay(pawn, FleckDefOf.FlashHollow, Vector3.zero, 1.5f);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null && !AbilityUtility.ValidateHasTendableWound(pawn, throwMessages, parent))
		{
			return false;
		}
		return base.Valid(target, throwMessages);
	}
}
