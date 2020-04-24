namespace RimWorld
{
	public class StorytellerCompProperties_SingleOnceFixed : StorytellerCompProperties
	{
		public IncidentDef incident;

		public int fireAfterDaysPassed;

		public RoyalTitleDef skipIfColonistHasMinTitle;

		public bool skipIfOnExtremeBiome;

		public int minColonistCount;

		public StorytellerCompProperties_SingleOnceFixed()
		{
			compClass = typeof(StorytellerComp_SingleOnceFixed);
		}
	}
}
