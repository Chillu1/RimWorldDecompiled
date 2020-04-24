namespace RimWorld
{
	public class StorytellerCompProperties_FactionInteraction : StorytellerCompProperties
	{
		public IncidentDef incident;

		public float baseIncidentsPerYear;

		public float minSpacingDays;

		public StoryDanger minDanger;

		public bool fullAlliesOnly;

		public StorytellerCompProperties_FactionInteraction()
		{
			compClass = typeof(StorytellerComp_FactionInteraction);
		}
	}
}
