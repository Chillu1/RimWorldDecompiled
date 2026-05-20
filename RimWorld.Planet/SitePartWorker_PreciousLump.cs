using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld.Planet;

public class SitePartWorker_PreciousLump : SitePartWorker
{
	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		if (site.MainSitePartDef == def)
		{
			return null;
		}
		return base.GetPostProcessedThreatLabel(site, sitePart);
	}

	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		if (part.site.ActualThreatPoints > 0f)
		{
			outExtraDescriptionRules.Add(new Rule_String("lumpThreatDescription", "\n\n" + "PreciousLumpHostileThreat".Translate()));
		}
		else
		{
			outExtraDescriptionRules.Add(new Rule_String("lumpThreatDescription", ""));
		}
	}
}
