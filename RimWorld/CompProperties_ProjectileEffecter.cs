using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_ProjectileEffecter : CompProperties
{
	public EffecterDef effecterDef;

	public CompProperties_ProjectileEffecter()
	{
		compClass = typeof(Comp_ProjectileEffecter);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (!typeof(Projectile).IsAssignableFrom(parentDef.thingClass))
		{
			yield return GetType().Name + " is only meant to be used on Projectile derived Things";
		}
	}
}
