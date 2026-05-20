namespace RimWorld;

public class StorytellerCompProperties_MonolithMigration : StorytellerCompProperties
{
	public float mtbDays;

	public IncidentDef incident;

	public StorytellerCompProperties_MonolithMigration()
	{
		compClass = typeof(StorytellerComp_MonolithMigration);
	}
}
