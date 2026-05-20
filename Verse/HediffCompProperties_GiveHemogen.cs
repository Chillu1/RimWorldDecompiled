namespace Verse
{
	public class HediffCompProperties_GiveHemogen : HediffCompProperties
	{
		public float amountPerDay;

		public HediffCompProperties_GiveHemogen()
		{
			compClass = typeof(HediffComp_GiveHemogen);
		}
	}
}
