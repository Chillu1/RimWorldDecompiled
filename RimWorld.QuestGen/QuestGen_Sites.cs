using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public static class QuestGen_Sites
{
	private const string RootSymbol = "root";

	public static QuestPart_SpawnWorldObject SpawnWorldObject(this Quest quest, WorldObject worldObject, List<ThingDef> defsToExcludeFromHyperlinks = null, string inSignal = null)
	{
		QuestPart_SpawnWorldObject questPart_SpawnWorldObject = new QuestPart_SpawnWorldObject();
		questPart_SpawnWorldObject.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_SpawnWorldObject.defsToExcludeFromHyperlinks = defsToExcludeFromHyperlinks;
		questPart_SpawnWorldObject.worldObject = worldObject;
		quest.AddPart(questPart_SpawnWorldObject);
		return questPart_SpawnWorldObject;
	}

	public static QuestPart_StartDetectionRaids StartRecurringRaids(this Quest quest, WorldObject worldObject, FloatRange? delayRangeHours = null, int? firstRaidDelayTicks = null, string inSignal = null)
	{
		QuestPart_StartDetectionRaids questPart_StartDetectionRaids = new QuestPart_StartDetectionRaids();
		questPart_StartDetectionRaids.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_StartDetectionRaids.worldObject = worldObject;
		questPart_StartDetectionRaids.delayRangeHours = delayRangeHours;
		questPart_StartDetectionRaids.firstRaidDelayTicks = firstRaidDelayTicks;
		quest.AddPart(questPart_StartDetectionRaids);
		return questPart_StartDetectionRaids;
	}

	public static Site GenerateSite(IEnumerable<SitePartDefWithParams> sitePartsParams, PlanetTile tile, Faction faction, bool hiddenSitePartsPossible = false, RulePack singleSitePartRules = null, WorldObjectDef worldObjectDef = null)
	{
		Slate slate = QuestGen.slate;
		bool flag = false;
		foreach (SitePartDefWithParams sitePartsParam in sitePartsParams)
		{
			if (sitePartsParam.def.defaultHidden)
			{
				flag = true;
				break;
			}
		}
		if (flag || hiddenSitePartsPossible)
		{
			SitePartParams parms = SitePartDefOf.PossibleUnknownThreatMarker.Worker.GenerateDefaultParams(0f, tile, faction);
			SitePartDefWithParams val = new SitePartDefWithParams(SitePartDefOf.PossibleUnknownThreatMarker, parms);
			sitePartsParams = sitePartsParams.Concat(Gen.YieldSingle(val));
		}
		Site site = SiteMaker.MakeSite(sitePartsParams, tile, faction, ifHostileThenMustRemainHostile: true, worldObjectDef);
		List<Rule> list = new List<Rule>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		List<string> list2 = new List<string>();
		int num = 0;
		for (int i = 0; i < site.parts.Count; i++)
		{
			List<Rule> list3 = new List<Rule>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			site.parts[i].def.Worker.Notify_GeneratedByQuestGen(site.parts[i], slate, list3, dictionary2);
			if (site.parts[i].hidden)
			{
				continue;
			}
			if (singleSitePartRules != null)
			{
				List<Rule> list4 = new List<Rule>();
				list4.AddRange(list3);
				list4.AddRange(singleSitePartRules.Rules);
				string text = QuestGenUtility.ResolveLocalText(list4, dictionary2, "root", capitalizeFirstSentence: false);
				list.Add(new Rule_String("sitePart" + num + "_description", text));
				if (!text.NullOrEmpty())
				{
					list2.Add(text);
				}
			}
			for (int j = 0; j < list3.Count; j++)
			{
				Rule rule = list3[j].DeepCopy();
				if (rule is Rule_String rule_String && num != 0)
				{
					rule_String.keyword = "sitePart" + num + "_" + rule_String.keyword;
				}
				list.Add(rule);
			}
			foreach (KeyValuePair<string, string> item in dictionary2)
			{
				string text2 = item.Key;
				if (num != 0)
				{
					text2 = "sitePart" + num + "_" + text2;
				}
				if (!dictionary.ContainsKey(text2))
				{
					dictionary.Add(text2, item.Value);
				}
			}
			num++;
		}
		if (!list2.Any())
		{
			list.Add(new Rule_String("allSitePartsDescriptions", "HiddenOrNoSitePartDescription".Translate()));
			list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", "HiddenOrNoSitePartDescription".Translate()));
		}
		else
		{
			list.Add(new Rule_String("allSitePartsDescriptions", list2.ToClauseSequence().Resolve()));
			if (list2.Count >= 2)
			{
				list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", list2.Skip(1).ToList().ToClauseSequence()
					.Resolve()));
			}
			else
			{
				list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", "HiddenOrNoSitePartDescription".Translate()));
			}
		}
		QuestGen.AddQuestDescriptionRules(list);
		QuestGen.AddQuestNameRules(list);
		QuestGen.AddQuestDescriptionConstants(dictionary);
		QuestGen.AddQuestNameConstants(dictionary);
		QuestGen.AddQuestNameRules(new List<Rule>
		{
			new Rule_String("site_label", site.Label)
		});
		return site;
	}

	public static QuestPart_SetSitePartThreatPointsToCurrent SetSitePartThreatPointsToCurrent(this Quest quest, Site site, SitePartDef sitePartDef, MapParent useMapParentThreatPoints, string inSignal = null, float threatPointsFactor = 1f)
	{
		QuestPart_SetSitePartThreatPointsToCurrent questPart_SetSitePartThreatPointsToCurrent = new QuestPart_SetSitePartThreatPointsToCurrent();
		questPart_SetSitePartThreatPointsToCurrent.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_SetSitePartThreatPointsToCurrent.site = site;
		questPart_SetSitePartThreatPointsToCurrent.sitePartDef = sitePartDef;
		questPart_SetSitePartThreatPointsToCurrent.useMapParentThreatPoints = useMapParentThreatPoints;
		questPart_SetSitePartThreatPointsToCurrent.threatPointsFactor = threatPointsFactor;
		quest.AddPart(questPart_SetSitePartThreatPointsToCurrent);
		return questPart_SetSitePartThreatPointsToCurrent;
	}
}
