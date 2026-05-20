using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorytellerComp_ImportantQuest : StorytellerComp
{
	private static int IntervalsPassed => Find.TickManager.TicksGame / 1000;

	private StorytellerCompProperties_ImportantQuest Props => (StorytellerCompProperties_ImportantQuest)props;

	private bool BeenGivenQuest => Find.QuestManager.QuestsListForReading.Any((Quest q) => q.root == Props.questDef);

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		if (IntervalsPassed > Props.fireAfterDaysPassed * 60 && !BeenGivenQuest)
		{
			IncidentDef questIncident = Props.questIncident;
			if (questIncident.TargetAllowed(target))
			{
				yield return new FiringIncident(questIncident, this, GenerateParms(questIncident.category, target));
			}
		}
	}
}
