using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_ConditionCauser_PsychicDroner : SitePartWorker_ConditionCauser
{
	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		string label = part.conditionCauser.TryGetComp<CompCauseGameCondition_PsychicEmanation>().gender.GetLabel();
		outExtraDescriptionRules.Add(new Rule_String("affectedGender", label));
	}
}
