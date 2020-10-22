using RimWorld.QuestGen;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_GiveQuest_Map : IncidentWorker_GiveQuest
	{
		protected override void GiveQuest(IncidentParms parms, QuestScriptDef questDef)
		{
			Slate slate = new Slate();
			slate.Set("points", parms.points);
			slate.Set("map", (Map)parms.target);
			QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate));
		}
	}
}
