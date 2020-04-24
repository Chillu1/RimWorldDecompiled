namespace RimWorld
{
	public class StorytellerComp_RandomQuest : StorytellerComp_OnOffCycle
	{
		public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = base.GenerateParms(incCat, target);
			incidentParms.questScriptDef = NaturalRandomQuestChooser.ChooseNaturalRandomQuest(incidentParms.points, target);
			return incidentParms;
		}
	}
}
