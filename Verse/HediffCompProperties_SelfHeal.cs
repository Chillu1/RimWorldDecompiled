namespace Verse
{
	public class HediffCompProperties_SelfHeal : HediffCompProperties
	{
		public int healIntervalTicksStanding = 50;

		public float healAmount = 1f;

		public HediffCompProperties_SelfHeal()
		{
			compClass = typeof(HediffComp_SelfHeal);
		}
	}
}
