namespace Verse
{
	public class HediffCompProperties_Infecter : HediffCompProperties
	{
		public float infectionChance = 0.5f;

		public HediffCompProperties_Infecter()
		{
			compClass = typeof(HediffComp_Infecter);
		}
	}
}
