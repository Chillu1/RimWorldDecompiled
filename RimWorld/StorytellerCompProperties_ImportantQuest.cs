namespace RimWorld;

public class StorytellerCompProperties_ImportantQuest : StorytellerCompProperties
{
	public IncidentDef questIncident;

	public QuestScriptDef questDef;

	public int fireAfterDaysPassed;

	public StorytellerCompProperties_ImportantQuest()
	{
		compClass = typeof(StorytellerComp_ImportantQuest);
	}
}
