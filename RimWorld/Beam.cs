using UnityEngine;
using Verse;

namespace RimWorld;

public class Beam : Bullet
{
	public override Vector3 ExactPosition => destination + Vector3.up * def.Altitude;

	public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
	{
		base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
		Vector3 offsetA = (ExactPosition - launcher.Position.ToVector3Shifted()).Yto0().normalized * def.projectile.beamStartOffset;
		if (def.projectile.beamMoteDef != null)
		{
			MoteMaker.MakeInteractionOverlay(def.projectile.beamMoteDef, launcher, usedTarget.ToTargetInfo(base.Map), offsetA, Vector3.zero);
		}
		ImpactSomething();
	}
}
