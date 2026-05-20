namespace RimWorld
{
	public class StorytellerCompProperties_WorkSite : StorytellerCompProperties
	{
		public IncidentDef incident;

		public float baseMtbDays = 5f;

		public StorytellerCompProperties_WorkSite()
		{
			compClass = typeof(StorytellerComp_WorkSite);
		}
	}
}
