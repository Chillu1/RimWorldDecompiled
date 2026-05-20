using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_Outpost : SitePartWorker
{
	public static readonly SimpleCurve ThreatPointsLootMarketValue = new SimpleCurve
	{
		new CurvePoint(100f, 200f),
		new CurvePoint(250f, 450f),
		new CurvePoint(800f, 1000f),
		new CurvePoint(10000f, 2000f)
	};

	public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
	{
		string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
		lookTargets = new LookTargets(map.Parent);
		return arrivedLetterPart;
	}

	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		int enemiesCount = GetEnemiesCount(part.site, part.parms);
		outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
		outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
	}

	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		if (site.Faction.IsPlayer)
		{
			return null;
		}
		return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "KnownSiteThreatEnemyCountAppend".Translate(GetEnemiesCount(site, sitePart.parms), "Enemies".Translate());
	}

	public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
	{
		SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
		sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
		sitePartParams.lootMarketValue = ThreatPointsLootMarketValue.Evaluate(sitePartParams.threatPoints);
		return sitePartParams;
	}

	protected int GetEnemiesCount(Site site, SitePartParams parms)
	{
		return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
		{
			tile = site.Tile,
			faction = site.Faction,
			groupKind = PawnGroupKindDefOf.Settlement,
			points = parms.threatPoints,
			inhabitants = true,
			seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms)
		}).Count();
	}

	protected string GetEnemiesLabel(Site site, int enemiesCount)
	{
		if (site.Faction == null)
		{
			return (enemiesCount == 1) ? "Enemy".Translate() : "Enemies".Translate();
		}
		if (enemiesCount != 1)
		{
			return site.Faction.def.pawnsPlural;
		}
		return site.Faction.def.pawnSingular;
	}
}
