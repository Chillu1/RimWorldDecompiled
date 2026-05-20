namespace Verse
{
	public class HediffCompProperties_RandomizeStageWithInterval : HediffCompProperties_Randomizer
	{
		[MustTranslate]
		public string notifyMessage;

		public HediffCompProperties_RandomizeStageWithInterval()
		{
			compClass = typeof(HediffComp_RandomizeStageWithInterval);
		}
	}
}
