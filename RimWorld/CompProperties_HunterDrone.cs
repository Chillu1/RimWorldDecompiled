using Verse;

namespace RimWorld;

public class CompProperties_HunterDrone : CompProperties
{
	public float explosionRadius = 1.9f;

	public DamageDef explosionDamageType;

	public int explosionDamageAmount = 50;

	public CompProperties_HunterDrone()
	{
		compClass = typeof(CompHunterDrone);
	}

	public override void ResolveReferences(ThingDef parentDef)
	{
		base.ResolveReferences(parentDef);
		if (explosionDamageType == null)
		{
			explosionDamageType = DamageDefOf.Bomb;
		}
	}
}
