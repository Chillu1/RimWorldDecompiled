using Verse;

namespace RimWorld
{
	public class CompProperties_GasOnDamage : CompProperties
	{
		public GasType type;

		public float damageFactor = 6f;

		public DamageDef damage;

		public bool useStackCountAsFactor;

		public CompProperties_GasOnDamage()
		{
			compClass = typeof(CompGasOnDamage);
		}
	}
}
