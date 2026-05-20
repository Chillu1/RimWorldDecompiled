using Verse;

namespace RimWorld;

public class BroadshieldPack : Apparel
{
	public override void Notify_BulletImpactNearby(BulletImpactData impactData)
	{
		base.Notify_BulletImpactNearby(impactData);
		Pawn wearer = base.Wearer;
		if (wearer != null && !wearer.Dead && !impactData.bullet.DamageDef.isExplosive && CompProjectileInterceptor.InterceptsProjectile(BroadshieldProjectileInterceptorProperties(), impactData.bullet) && impactData.bullet.Launcher != null && impactData.bullet.Launcher.HostileTo(base.Wearer) && !wearer.IsColonist && wearer.Spawned && !NearbyActiveBroadshield())
		{
			Verb_DeployBroadshield.Deploy(this.TryGetComp<CompApparelReloadable>());
		}
	}

	private bool NearbyActiveBroadshield()
	{
		float radius = BroadshieldProjectileInterceptorProperties().radius;
		foreach (Thing item in GenRadial.RadialDistinctThingsAround(base.PositionHeld, base.MapHeld, radius, useCenter: true))
		{
			CompProjectileInterceptor compProjectileInterceptor = item.TryGetComp<CompProjectileInterceptor>();
			if (item.def == ThingDefOf.BroadshieldProjector && compProjectileInterceptor != null && compProjectileInterceptor.Active)
			{
				return true;
			}
		}
		return false;
	}

	private static CompProperties_ProjectileInterceptor BroadshieldProjectileInterceptorProperties()
	{
		return ThingDefOf.BroadshieldProjector.GetCompProperties<CompProperties_ProjectileInterceptor>();
	}
}
