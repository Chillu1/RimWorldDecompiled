using System.Collections.Generic;

namespace RimWorld
{
	public class StorytellerCompProperties_MechanitorComplexQuest : StorytellerCompProperties
	{
		public IncidentDef incident;

		public int mtbDays = 60;

		public float minSpacingDays;

		public float existingMechanitorOrMechlinkMTBFactor = 1f;

		public List<QuestScriptDef> blockedByQueuedOrActiveQuests;

		public StorytellerCompProperties_MechanitorComplexQuest()
		{
			compClass = typeof(StorytellerComp_MechanitorComplexQuest);
		}
	}
}
