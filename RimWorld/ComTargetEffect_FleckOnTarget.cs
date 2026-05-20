using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ComTargetEffect_FleckOnTarget : CompTargetEffect
	{
		private CompProperties_TargetEffect_FleckOnTarget Props => (CompProperties_TargetEffect_FleckOnTarget)props;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (Props.fleckDef != null)
			{
				FleckMaker.AttachedOverlay(target, Props.fleckDef, Vector3.zero);
			}
		}
	}
}
