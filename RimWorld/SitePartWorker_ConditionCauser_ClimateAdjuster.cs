using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_ConditionCauser_ClimateAdjuster : SitePartWorker_ConditionCauser
{
	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		string output = part.conditionCauser.TryGetComp<CompCauseGameCondition_TemperatureOffset>().temperatureOffset.ToStringTemperatureOffset();
		outExtraDescriptionRules.Add(new Rule_String("temperatureOffset", output));
	}
}
