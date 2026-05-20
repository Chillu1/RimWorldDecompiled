using Verse;

namespace RimWorld;

public class CompAbilityEffect_LaunchProjectile : CompAbilityEffect
{
	public new CompProperties_AbilityLaunchProjectile Props => (CompProperties_AbilityLaunchProjectile)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		LaunchProjectile(target);
	}

	private void LaunchProjectile(LocalTargetInfo target)
	{
		if (Props.projectileDef != null)
		{
			Pawn pawn = parent.pawn;
			((Projectile)GenSpawn.Spawn(Props.projectileDef, pawn.Position, pawn.Map)).Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget, parent.verb.preventFriendlyFire);
		}
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return target.Pawn != null;
	}
}
