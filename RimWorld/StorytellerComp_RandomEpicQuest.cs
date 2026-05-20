using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class StorytellerComp_RandomEpicQuest : StorytellerComp_OnOffCycle
{
	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
		int num = 0;
		while (true)
		{
			if (num < questsListForReading.Count)
			{
				if (!questsListForReading[num].root.IsEpic || (questsListForReading[num].State != QuestState.NotYetAccepted && questsListForReading[num].State != QuestState.Ongoing))
				{
					num++;
					continue;
				}
				break;
			}
			foreach (FiringIncident item in base.MakeIntervalIncidents(target))
			{
				yield return item;
			}
			break;
		}
	}

	public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
	{
		IncidentParms parms = base.GenerateParms(incCat, target);
		if (DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef x) => x.IsEpic && x.CanRun(parms.points, target)).TryRandomElement(out var result))
		{
			parms.questScriptDef = result;
		}
		return parms;
	}
}
