using Verse;

namespace RimWorld
{
	public class IncidentWorker_GiveQuest : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			if (def.questScriptDef != null)
			{
				if (!def.questScriptDef.CanRun(parms.points))
				{
					return false;
				}
			}
			else if (parms.questScriptDef != null && !parms.questScriptDef.CanRun(parms.points))
			{
				return false;
			}
			return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Any();
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(def.questScriptDef ?? parms.questScriptDef ?? NaturalRandomQuestChooser.ChooseNaturalRandomQuest(parms.points, parms.target), parms.points);
			if (!quest.hidden)
			{
				QuestUtility.SendLetterQuestAvailable(quest);
			}
			return true;
		}

		protected virtual void GiveQuest(IncidentParms parms, QuestScriptDef questDef)
		{
			QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(questDef, parms.points));
		}
	}
}
