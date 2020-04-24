using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Smokepop : CompAbilityEffect
	{
		public new CompProperties_AbilitySmokepop Props => (CompProperties_AbilitySmokepop)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, Props.smokeRadius, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, ThingDefOf.Gas_Smoke, 1f);
		}

		public override void DrawEffectPreview(LocalTargetInfo target)
		{
			GenDraw.DrawRadiusRing(target.Cell, Props.smokeRadius);
		}
	}
}
