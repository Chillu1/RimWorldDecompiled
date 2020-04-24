using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class NaturalRandomQuestChooser
	{
		public static float PopulationIncreasingQuestChance()
		{
			return QuestTuning.IncreasesPopQuestChanceByPopIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntentForQuest);
		}

		public static QuestScriptDef ChooseNaturalRandomQuest(float points, IIncidentTarget target)
		{
			bool flag = Rand.Chance(PopulationIncreasingQuestChance());
			if (TryGetQuest(flag, out QuestScriptDef chosen2))
			{
				return chosen2;
			}
			if (flag && TryGetQuest(incPop: false, out QuestScriptDef chosen3))
			{
				return chosen3;
			}
			Log.Error("Couldn't find any random quest. points=" + points);
			return null;
			bool TryGetQuest(bool incPop, out QuestScriptDef chosen)
			{
				return DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef x) => x.IsRootRandomSelected && x.rootIncreasesPopulation == incPop && x.CanRun(points)).TryRandomElementByWeight((QuestScriptDef x) => GetNaturalRandomSelectionWeight(x, points, target.StoryState), out chosen);
			}
		}

		public static float GetNaturalRandomSelectionWeight(QuestScriptDef quest, float points, StoryState storyState)
		{
			if (quest.rootSelectionWeight <= 0f || points < quest.rootMinPoints)
			{
				return 0f;
			}
			float num = quest.rootSelectionWeight;
			for (int i = 0; i < storyState.RecentRandomQuests.Count; i++)
			{
				if (storyState.RecentRandomQuests[i] == quest)
				{
					switch (i)
					{
					case 0:
						num *= 0.01f;
						break;
					case 1:
						num *= 0.3f;
						break;
					case 2:
						num *= 0.5f;
						break;
					case 3:
						num *= 0.7f;
						break;
					case 4:
						num *= 0.9f;
						break;
					}
				}
			}
			if (!quest.canGiveRoyalFavor && PlayerWantsRoyalFavorFromAnyFaction())
			{
				int num2 = (storyState.LastRoyalFavorQuestTick != -1) ? storyState.LastRoyalFavorQuestTick : 0;
				float x = (float)(Find.TickManager.TicksGame - num2) / 60000f;
				num *= QuestTuning.NonFavorQuestSelectionWeightFactorByDaysSinceFavorQuestCurve.Evaluate(x);
			}
			return num;
			bool PlayerWantsRoyalFavorFromAnyFaction()
			{
				List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
				for (int j = 0; j < allFactionsListForReading.Count; j++)
				{
					if (allFactionsListForReading[j].allowRoyalFavorRewards && allFactionsListForReading[j] != Faction.OfPlayer && allFactionsListForReading[j].def.HasRoyalTitles)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static float GetNaturalDecreeSelectionWeight(QuestScriptDef quest, StoryState storyState)
		{
			if (quest.decreeSelectionWeight <= 0f)
			{
				return 0f;
			}
			float num = quest.decreeSelectionWeight;
			for (int i = 0; i < storyState.RecentRandomDecrees.Count; i++)
			{
				if (storyState.RecentRandomDecrees[i] == quest)
				{
					switch (i)
					{
					case 0:
						num *= 0.01f;
						break;
					case 1:
						num *= 0.3f;
						break;
					case 2:
						num *= 0.5f;
						break;
					case 3:
						num *= 0.7f;
						break;
					case 4:
						num *= 0.9f;
						break;
					}
				}
			}
			return num;
		}

		public static float DebugTotalNaturalRandomSelectionWeight(QuestScriptDef quest, float points, IIncidentTarget target)
		{
			if (!quest.IsRootRandomSelected)
			{
				return 0f;
			}
			if (!quest.CanRun(points))
			{
				return 0f;
			}
			float naturalRandomSelectionWeight = GetNaturalRandomSelectionWeight(quest, points, target.StoryState);
			float num = QuestTuning.IncreasesPopQuestChanceByPopIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntentForQuest);
			return num * (quest.rootIncreasesPopulation ? naturalRandomSelectionWeight : 0f) + (1f - num) * ((!quest.rootIncreasesPopulation) ? naturalRandomSelectionWeight : 0f);
		}
	}
}
