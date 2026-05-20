using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Reward_RelicInfo : Reward
	{
		public Precept_Relic relic;

		public Quest quest;

		public override IEnumerable<GenUI.AnonymousStackElement> StackElements
		{
			get
			{
				yield return QuestPartUtility.GetStandardRewardStackElement("Reward_RelicInfoLabel".Translate(relic.Label), delegate(Rect rect)
				{
					relic.DrawIcon(rect);
				}, () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst() + ".", delegate
				{
					if (quest?.parent != null)
					{
						Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
						((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest.parent);
					}
				});
			}
		}

		public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
		{
			throw new NotImplementedException();
		}

		public override string GetDescription(RewardsGeneratorParams parms)
		{
			return "Reward_RelicInfo".Translate(relic.Label);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref relic, "relic");
			Scribe_References.Look(ref quest, "quest");
		}
	}
}
