namespace RimWorld
{
	public class StorytellerCompProperties_SingleMTB : StorytellerCompProperties
	{
		public IncidentDef incident;

		public float mtbDays = 13f;

		public StorytellerCompProperties_SingleMTB()
		{
			compClass = typeof(StorytellerComp_SingleMTB);
		}
	}
}
