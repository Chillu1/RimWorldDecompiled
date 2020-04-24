using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class SitePartWorker_Turrets : SitePartWorker
	{
		private const int MinTurrets = 2;

		private const int MaxTurrets = 11;

		private List<string> threatsTmp = new List<string>();

		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			lookTargets = (map.listerThings.AllThings.FirstOrDefault((Thing x) => x is Building_TurretGun && x.HostileTo(Faction.OfPlayer)) ?? map.listerThings.AllThings.FirstOrDefault((Thing x) => x is Building_TurretGun));
			return arrivedLetterPart;
		}

		public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
		{
			SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
			sitePartParams.mortarsCount = Rand.RangeInclusive(0, 1);
			sitePartParams.turretsCount = Mathf.Clamp(Mathf.RoundToInt(sitePartParams.threatPoints / ThingDefOf.Turret_MiniTurret.building.combatPower), 2, 11);
			return sitePartParams;
		}

		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			string threatsInfo = GetThreatsInfo(part.parms);
			outExtraDescriptionRules.Add(new Rule_String("threatsInfo", threatsInfo));
		}

		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + GetThreatsInfo(sitePart.parms);
		}

		private string GetThreatsInfo(SitePartParams parms)
		{
			threatsTmp.Clear();
			int num = parms.mortarsCount + 1;
			string text = null;
			if (parms.turretsCount != 0)
			{
				text = ((parms.turretsCount != 1) ? ((string)"Turrets".Translate()) : ((string)"Turret".Translate()));
				threatsTmp.Add("KnownSiteThreatEnemyCountAppend".Translate(parms.turretsCount.ToString(), text));
			}
			if (parms.mortarsCount != 0)
			{
				text = ((parms.mortarsCount != 1) ? ((string)"Mortars".Translate()) : ((string)"Mortar".Translate()));
				threatsTmp.Add("KnownSiteThreatEnemyCountAppend".Translate(parms.mortarsCount.ToString(), text));
			}
			if (num != 0)
			{
				text = ((num != 1) ? ((string)"Enemies".Translate()) : ((string)"Enemy".Translate()));
				threatsTmp.Add("KnownSiteThreatEnemyCountAppend".Translate(num.ToString(), text));
			}
			return threatsTmp.ToCommaList(useAnd: true);
		}
	}
}
