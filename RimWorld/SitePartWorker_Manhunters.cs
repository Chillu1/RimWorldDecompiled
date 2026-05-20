using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_Manhunters : SitePartWorker
{
	public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
	{
		string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
		lookTargets = new LookTargets(map.Parent);
		return arrivedLetterPart;
	}

	public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
	{
		SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
		if (ManhunterPackGenStepUtility.TryGetAnimalsKind(sitePartParams.threatPoints, tile, out sitePartParams.animalKind))
		{
			sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, sitePartParams.animalKind.combatPower);
		}
		return sitePartParams;
	}

	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		int animalsCount = GetAnimalsCount(part.parms);
		string output = GenLabel.BestKindLabel(part.parms.animalKind, Gender.None, plural: true, animalsCount);
		outExtraDescriptionRules.Add(new Rule_String("count", animalsCount.ToString()));
		outExtraDescriptionRules.Add(new Rule_String("kindLabel", output));
		outExtraDescriptionConstants.Add("count", animalsCount.ToString());
	}

	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		int animalsCount = GetAnimalsCount(sitePart.parms);
		return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "KnownSiteThreatEnemyCountAppend".Translate(animalsCount.ToString(), GenLabel.BestKindLabel(sitePart.parms.animalKind, Gender.None, plural: true, animalsCount));
	}

	private int GetAnimalsCount(SitePartParams parms)
	{
		return AggressiveAnimalIncidentUtility.GetAnimalsCount(parms.animalKind, parms.threatPoints);
	}
}
