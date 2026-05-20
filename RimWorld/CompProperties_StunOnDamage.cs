using Verse;

namespace RimWorld
{
	public class CompProperties_StunOnDamage : CompProperties
	{
		public int delayTicks;

		public DamageDef damage;

		public CompProperties_StunOnDamage()
		{
			compClass = typeof(CompStunOnDamage);
		}
	}
}
