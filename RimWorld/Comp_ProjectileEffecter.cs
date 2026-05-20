using Verse;

namespace RimWorld;

public class Comp_ProjectileEffecter : ThingComp
{
	private Effecter effecter;

	public CompProperties_ProjectileEffecter Props => (CompProperties_ProjectileEffecter)props;

	public override void CompTick()
	{
		base.CompTick();
		if (parent.Spawned)
		{
			Projectile projectile = parent as Projectile;
			if (effecter == null)
			{
				effecter = Props.effecterDef.Spawn(projectile.Position, projectile.intendedTarget.Cell, parent.MapHeld);
			}
			effecter?.EffectTick(parent, parent);
		}
		else
		{
			effecter?.Cleanup();
			effecter = null;
		}
	}
}
