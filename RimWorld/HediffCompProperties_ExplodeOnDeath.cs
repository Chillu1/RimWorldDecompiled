using Verse;

namespace RimWorld
{
	public class HediffCompProperties_ExplodeOnDeath : HediffCompProperties
	{
		public bool destroyGear;

		public bool destroyBody;

		public float explosionRadius;

		public DamageDef damageDef;

		public int damageAmount = -1;

		public HediffCompProperties_ExplodeOnDeath()
		{
			compClass = typeof(HediffComp_ExplodeOnDeath);
		}
	}
}
