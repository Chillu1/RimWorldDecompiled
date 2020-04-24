using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateSite : QuestNode
	{
		public SlateRef<IEnumerable<SitePartDefWithParams>> sitePartsParams;

		public SlateRef<Faction> faction;

		public SlateRef<int> tile;

		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<RulePack> singleSitePartRules;

		public SlateRef<bool> hiddenSitePartsPossible;

		private const string RootSymbol = "root";

		protected override bool TestRunInt(Slate slate)
		{
			if (!Find.Storyteller.difficulty.allowViolentQuests && sitePartsParams.GetValue(slate) != null)
			{
				foreach (SitePartDefWithParams item in sitePartsParams.GetValue(slate))
				{
					if (item.def.wantsThreatPoints)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			IEnumerable<SitePartDefWithParams> enumerable = sitePartsParams.GetValue(slate);
			bool flag = false;
			foreach (SitePartDefWithParams item in enumerable)
			{
				if (item.def.defaultHidden)
				{
					flag = true;
					break;
				}
			}
			if (flag || hiddenSitePartsPossible.GetValue(slate))
			{
				SitePartParams parms = SitePartDefOf.PossibleUnknownThreatMarker.Worker.GenerateDefaultParams(0f, tile.GetValue(slate), faction.GetValue(slate));
				SitePartDefWithParams val = new SitePartDefWithParams(SitePartDefOf.PossibleUnknownThreatMarker, parms);
				enumerable = enumerable.Concat(Gen.YieldSingle(val));
			}
			Site site = SiteMaker.MakeSite(enumerable, tile.GetValue(slate), faction.GetValue(slate));
			List<Rule> list = new List<Rule>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			List<string> list2 = new List<string>();
			int num = 0;
			for (int i = 0; i < site.parts.Count; i++)
			{
				List<Rule> list3 = new List<Rule>();
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				site.parts[i].def.Worker.Notify_GeneratedByQuestGen(site.parts[i], QuestGen.slate, list3, dictionary2);
				if (site.parts[i].hidden)
				{
					continue;
				}
				if (singleSitePartRules.GetValue(slate) != null)
				{
					List<Rule> list4 = new List<Rule>();
					list4.AddRange(list3);
					list4.AddRange(singleSitePartRules.GetValue(slate).Rules);
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
					Rule_String rule_String = rule as Rule_String;
					if (rule_String != null && num != 0)
					{
						rule_String.keyword = "sitePart" + num + "_" + rule_String.keyword;
					}
					list.Add(rule);
				}
				foreach (KeyValuePair<string, string> item2 in dictionary2)
				{
					string text2 = item2.Key;
					if (num != 0)
					{
						text2 = "sitePart" + num + "_" + text2;
					}
					if (!dictionary.ContainsKey(text2))
					{
						dictionary.Add(text2, item2.Value);
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
				list.Add(new Rule_String("allSitePartsDescriptions", list2.ToClauseSequence()));
				if (list2.Count >= 2)
				{
					list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", list2.Skip(1).ToList().ToClauseSequence()));
				}
				else
				{
					list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", "HiddenOrNoSitePartDescription".Translate()));
				}
			}
			if (storeAs.GetValue(slate) != null)
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), site);
			}
			QuestGen.AddQuestDescriptionRules(list);
			QuestGen.AddQuestNameRules(list);
			QuestGen.AddQuestDescriptionConstants(dictionary);
			QuestGen.AddQuestNameConstants(dictionary);
			QuestGen.AddQuestNameRules(new List<Rule>
			{
				new Rule_String("site_label", site.Label)
			});
		}
	}
}
