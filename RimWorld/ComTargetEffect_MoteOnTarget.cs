using UnityEngine;
using Verse;

namespace RimWorld;

public class ComTargetEffect_MoteOnTarget : CompTargetEffect
{
	private CompProperties_TargetEffect_MoteOnTarget Props => (CompProperties_TargetEffect_MoteOnTarget)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (Props.moteDef != null)
		{
			MoteMaker.MakeAttachedOverlay(target, Props.moteDef, Vector3.zero);
		}
	}
}
