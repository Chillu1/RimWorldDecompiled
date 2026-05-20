using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Reward_DevelopmentPoints : Reward
	{
		public Quest quest;

		public override IEnumerable<GenUI.AnonymousStackElement> StackElements
		{
			get
			{
				Ideo fluidIdeo = Faction.OfPlayer.ideos.FluidIdeo;
				if (fluidIdeo == null || !fluidIdeo.development.CanBeDevelopedNow)
				{
					yield break;
				}
				int num = IdeoDevelopmentUtility.DevelopmentPointsForQuestSuccess(fluidIdeo, quest.root);
				if (num > 0)
				{
					yield return QuestPartUtility.GetStandardRewardStackElement("Reward_DevelopmentPointsLabel".Translate() + ": " + num.ToStringWithSign(), delegate(Rect rect)
					{
						fluidIdeo.DrawIcon(rect);
					}, () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst(), delegate
					{
						Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Ideos);
						IdeoUIUtility.SetSelected(fluidIdeo);
					});
				}
			}
		}

		public Reward_DevelopmentPoints()
		{
		}

		public Reward_DevelopmentPoints(Quest quest)
		{
			this.quest = quest;
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
			Ideo fluidIdeo = Faction.OfPlayer.ideos.FluidIdeo;
			int num = IdeoDevelopmentUtility.DevelopmentPointsForQuestSuccess(fluidIdeo, quest.root);
			return "Reward_DevelopmentPoints".Translate(fluidIdeo, num, fluidIdeo.development.points, fluidIdeo.development.NextReformationDevelopmentPoints).Resolve();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref quest, "quest");
		}
	}
}
