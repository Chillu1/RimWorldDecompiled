using System.Collections.Generic;
using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class StorytellerUtilityPopulation
{
	private static float PopulationValue_Prisoner = 0.5f;

	private static float PopulationValue_PrisonerUnrecruitable = 0.25f;

	private static float PopulationValue_Slave = 0.75f;

	private static float PopulationValue_Child = 0.3f;

	private static StorytellerDef StorytellerDef => Find.Storyteller.def;

	public static float PopulationIntent => CalculatePopulationIntent(StorytellerDef, AdjustedPopulation, Find.StoryWatcher.watcherPopAdaptation.AdaptDays);

	public static float PopulationIntentForQuest => CalculatePopulationIntent(StorytellerDef, AdjustedPopulationIncludingQuests, Find.StoryWatcher.watcherPopAdaptation.AdaptDays);

	public static float AdjustedPopulation
	{
		get
		{
			float num = 0f;
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
			{
				if (item.IsColonist || item.IsPrisonerOfColony || item.IsSlaveOfColony)
				{
					num += AdjustedPopulationValue(item);
				}
			}
			return num + (float)QuestUtility.TotalBorrowedColonistCount();
		}
	}

	public static float AdjustedPopulationIncludingQuests
	{
		get
		{
			float num = AdjustedPopulation;
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (!questsListForReading[i].Historical && questsListForReading[i].IncreasesPopulation)
				{
					num += 1f;
				}
			}
			return num;
		}
	}

	private static float AdjustedPopulationValue(Pawn pawn)
	{
		if (pawn.DevelopmentalStage.Baby())
		{
			return 0f;
		}
		float num = (pawn.DevelopmentalStage.Adult() ? 1f : Mathf.Lerp(PopulationValue_Child, 1f, (float)pawn.ageTracker.AgeBiologicalYears / pawn.ageTracker.AdultMinAge));
		if (pawn.IsPrisonerOfColony)
		{
			num = ((!pawn.guest.Recruitable) ? (num * PopulationValue_PrisonerUnrecruitable) : (num * PopulationValue_Prisoner));
		}
		else if (pawn.IsSlaveOfColony)
		{
			num *= PopulationValue_Slave;
		}
		return num;
	}

	private static float CalculatePopulationIntent(StorytellerDef def, float curPop, float popAdaptation)
	{
		float num = def.populationIntentFactorFromPopCurve.Evaluate(curPop);
		if (num > 0f)
		{
			num *= def.populationIntentFactorFromPopAdaptDaysCurve.Evaluate(popAdaptation);
		}
		return num;
	}

	public static string DebugReadout()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Population intent: ".PadRight(40) + PopulationIntent.ToString("F2"));
		stringBuilder.AppendLine("Population intent for quest: ".PadRight(40) + PopulationIntentForQuest.ToString("F2"));
		stringBuilder.AppendLine("Chance random quest increases population: ".PadRight(40) + NaturalRandomQuestChooser.PopulationIncreasingQuestChance().ToStringPercent());
		stringBuilder.AppendLine("Adjusted population: ".PadRight(40) + AdjustedPopulation.ToString("F1"));
		stringBuilder.AppendLine("Adjusted population including quests: ".PadRight(40) + AdjustedPopulation.ToString("F1"));
		stringBuilder.AppendLine("Pop adaptation days: ".PadRight(40) + Find.StoryWatcher.watcherPopAdaptation.AdaptDays.ToString("F2"));
		return stringBuilder.ToString();
	}

	[DebugOutput]
	public static void PopulationIntents()
	{
		List<float> list = new List<float>();
		for (int i = 0; i < 30; i++)
		{
			list.Add(i);
		}
		List<float> list2 = new List<float>();
		for (int j = 0; j < 40; j += 2)
		{
			list2.Add(j);
		}
		DebugTables.MakeTablesDialog(list2, (float ds) => "d-" + ds.ToString("F0"), list, (float rv) => rv.ToString("F2"), (float ds, float p) => CalculatePopulationIntent(StorytellerDef, p, (int)ds).ToString("F2"), "pop");
	}
}
