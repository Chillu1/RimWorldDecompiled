using Verse;

namespace RimWorld;

public class CompAbilityEffect_Firefoampop : CompAbilityEffect
{
	public new CompProperties_AbilityFirefoampop Props => (CompProperties_AbilityFirefoampop)props;

	public bool ShouldHaveInspectString
	{
		get
		{
			if (ModsConfig.BiotechActive && parent.pawn.RaceProps.IsMechanoid)
			{
				return parent.pawn.IsColonyMech;
			}
			return false;
		}
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Effecter effecter = EffecterDefOf.ExtinguisherExplosion.Spawn();
		effecter.Trigger(new TargetInfo(target.Cell, parent.pawn.MapHeld), new TargetInfo(target.Cell, parent.pawn.MapHeld));
		effecter.Cleanup();
		GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, Props.firefoamRadius, DamageDefOf.Extinguish, null, -1, -1f, SoundDefOf.Explosion_FirefoamPopper, null, null, null, ThingDefOf.Filth_FireFoam, 1f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: true);
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		GenDraw.DrawRadiusRing(target.Cell, Props.firefoamRadius);
	}

	public override string CompInspectStringExtra()
	{
		if (ShouldHaveInspectString)
		{
			if ((bool)parent.CanCast)
			{
				return "AbilityMechFirefoampopCharged".Translate();
			}
			return "AbilityMechFirefoampopRecharging".Translate(parent.CooldownTicksRemaining.ToStringTicksToPeriod());
		}
		return null;
	}
}
