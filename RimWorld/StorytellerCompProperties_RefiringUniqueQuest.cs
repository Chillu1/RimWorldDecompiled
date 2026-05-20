namespace RimWorld
{
	public class StorytellerCompProperties_RefiringUniqueQuest : StorytellerCompProperties
	{
		public IncidentDef incident;

		public float refireEveryDays = -1f;

		public int minColonyWealth = -1;

		public StorytellerCompProperties_RefiringUniqueQuest()
		{
			compClass = typeof(StorytellerComp_RefiringUniqueQuest);
		}
	}
}
