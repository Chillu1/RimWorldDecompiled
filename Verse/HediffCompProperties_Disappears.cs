namespace Verse
{
	public class HediffCompProperties_Disappears : HediffCompProperties
	{
		public IntRange disappearsAfterTicks;

		public bool showRemainingTime;

		public HediffCompProperties_Disappears()
		{
			compClass = typeof(HediffComp_Disappears);
		}
	}
}
