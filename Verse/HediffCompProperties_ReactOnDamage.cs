namespace Verse
{
	public class HediffCompProperties_ReactOnDamage : HediffCompProperties
	{
		public DamageDef damageDefIncoming;

		public BodyPartDef createHediffOn;

		public HediffDef createHediff;

		public bool vomit;

		public HediffCompProperties_ReactOnDamage()
		{
			compClass = typeof(HediffComp_ReactOnDamage);
		}
	}
}
