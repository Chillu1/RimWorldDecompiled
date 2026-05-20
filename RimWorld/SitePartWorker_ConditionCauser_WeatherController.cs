using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_ConditionCauser_WeatherController : SitePartWorker_ConditionCauser
{
	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		WeatherDef weather = part.conditionCauser.TryGetComp<CompCauseGameCondition_ForceWeather>().weather;
		outExtraDescriptionRules.AddRange(GrammarUtility.RulesForDef("weather", weather));
	}
}
