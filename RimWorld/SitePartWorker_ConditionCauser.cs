using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_ConditionCauser : SitePartWorker
{
	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		int worldRange = sitePart.def.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange;
		return base.GetPostProcessedThreatLabel(site, sitePart) + " (" + "ConditionCauserRadius".Translate(worldRange) + ")";
	}

	public override void Init(Site site, SitePart sitePart)
	{
		sitePart.conditionCauser = ThingMaker.MakeThing(sitePart.def.conditionCauserDef);
		sitePart.conditionCauser.SetFaction(site.Faction);
		CompCauseGameCondition compCauseGameCondition = sitePart.conditionCauser.TryGetComp<CompCauseGameCondition>();
		compCauseGameCondition.RandomizeSettings(site);
		compCauseGameCondition.LinkWithSite(sitePart.site);
	}

	public override void SitePartWorkerTick(SitePart sitePart)
	{
		if (!sitePart.conditionCauser.DestroyedOrNull() && !sitePart.conditionCauser.Spawned)
		{
			sitePart.conditionCauser.DoTick();
		}
	}

	public override void PostDrawExtraSelectionOverlays(SitePart sitePart)
	{
		base.PostDrawExtraSelectionOverlays(sitePart);
		GenDraw.DrawWorldRadiusRing(sitePart.site.Tile, sitePart.def.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange);
	}

	public override void Notify_SiteMapAboutToBeRemoved(SitePart sitePart)
	{
		base.Notify_SiteMapAboutToBeRemoved(sitePart);
		if (!sitePart.conditionCauser.DestroyedOrNull() && sitePart.conditionCauser.Spawned && sitePart.conditionCauser.Map == sitePart.site.Map)
		{
			sitePart.conditionCauser.DeSpawn();
			sitePart.conditionCauserWasSpawned = false;
		}
	}

	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		slate.Set("conditionCauser", part.conditionCauser);
		outExtraDescriptionRules.Add(new Rule_String("problemCauserLabel", part.conditionCauser.Label));
	}
}
