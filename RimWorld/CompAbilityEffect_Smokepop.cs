using Verse;

namespace RimWorld;

public class CompAbilityEffect_Smokepop : CompAbilityEffect
{
	public new CompProperties_AbilitySmokepop Props => (CompProperties_AbilitySmokepop)props;

	public bool ShouldHaveInspectString
	{
		get
		{
			if (ModsConfig.BiotechActive)
			{
				return parent.pawn.RaceProps.IsMechanoid;
			}
			return false;
		}
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, Props.smokeRadius, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke);
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		GenDraw.DrawRadiusRing(target.Cell, Props.smokeRadius);
	}

	public override string CompInspectStringExtra()
	{
		if (ShouldHaveInspectString)
		{
			if ((bool)parent.CanCast)
			{
				return "AbilityMechSmokepopCharged".Translate();
			}
			return "AbilityMechSmokepopRecharging".Translate(parent.CooldownTicksRemaining.ToStringTicksToPeriod());
		}
		return null;
	}
}
