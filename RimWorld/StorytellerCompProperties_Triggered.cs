namespace RimWorld
{
	public class StorytellerCompProperties_Triggered : StorytellerCompProperties
	{
		public IncidentDef incident;

		public int delayTicks = 60;

		public StorytellerCompProperties_Triggered()
		{
			compClass = typeof(StorytellerComp_Triggered);
		}
	}
}
