using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRandomNegativeGameCondition : QuestNode
{
	private struct Option
	{
		public GameConditionDef gameCondition;

		public FloatRange durationDaysRange;

		public float difficulty;

		public int challengeRating;

		public Option(GameConditionDef gameCondition, FloatRange durationDaysRange, float difficulty, int challengeRating)
		{
			this.gameCondition = gameCondition;
			this.durationDaysRange = durationDaysRange;
			this.difficulty = difficulty;
			this.challengeRating = challengeRating;
		}
	}

	[NoTranslate]
	public SlateRef<string> storeGameConditionAs;

	[NoTranslate]
	public SlateRef<string> storeGameConditionDurationAs;

	[NoTranslate]
	public SlateRef<string> storeGameConditionDifficultyAs;

	private static List<Option> options;

	public static void ResetStaticData()
	{
		options = new List<Option>
		{
			new Option(GameConditionDefOf.VolcanicWinter, new FloatRange(10f, 20f), 0.4f, 1),
			new Option(GameConditionDefOf.WeatherController, new FloatRange(5f, 20f), 0.4f, 1),
			new Option(GameConditionDefOf.HeatWave, new FloatRange(4f, 8f), 1f, 1),
			new Option(GameConditionDefOf.ColdSnap, new FloatRange(4f, 8f), 1f, 1),
			new Option(GameConditionDefOf.ToxicFallout, new FloatRange(5f, 20f), 0.8f, 2),
			new Option(GameConditionDefOf.PsychicSuppression, new FloatRange(4f, 8f), 1.5f, 2),
			new Option(GameConditionDefOf.EMIField, new FloatRange(4f, 8f), 1.8f, 3),
			new Option(GameConditionDefOf.PsychicDrone, new FloatRange(4f, 8f), 2f, 3)
		};
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (slate.Get<Map>("map") == null)
		{
			return false;
		}
		return DoWork(slate);
	}

	protected override void RunInt()
	{
		DoWork(QuestGen.slate);
	}

	private bool DoWork(Slate slate)
	{
		Option result;
		if (QuestGen.Working)
		{
			if (!options.Where((Option x) => x.challengeRating == QuestGen.quest.challengeRating && PossibleNow(x.gameCondition, slate)).TryRandomElement(out result) && !options.Where((Option x) => x.challengeRating < QuestGen.quest.challengeRating && PossibleNow(x.gameCondition, slate)).TryRandomElement(out result) && !options.Where((Option x) => PossibleNow(x.gameCondition, slate)).TryRandomElement(out result))
			{
				return false;
			}
		}
		else if (!options.Where((Option x) => PossibleNow(x.gameCondition, slate)).TryRandomElement(out result))
		{
			return false;
		}
		int var = (int)(result.durationDaysRange.RandomInRange * 60000f);
		slate.Set(storeGameConditionAs.GetValue(slate), result.gameCondition);
		slate.Set(storeGameConditionDurationAs.GetValue(slate), var);
		slate.Set(storeGameConditionDifficultyAs.GetValue(slate), result.difficulty);
		return true;
	}

	private bool PossibleNow(GameConditionDef def, Slate slate)
	{
		if (def == null)
		{
			return false;
		}
		Map map = slate.Get<Map>("map");
		if (map.gameConditionManager.ConditionIsActive(def))
		{
			return false;
		}
		IncidentDef incidentDef = null;
		List<IncidentDef> allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (allDefsListForReading[i].Worker is IncidentWorker_MakeGameCondition && allDefsListForReading[i].gameCondition == def)
			{
				incidentDef = allDefsListForReading[i];
				break;
			}
		}
		if (incidentDef != null)
		{
			if (!Find.Storyteller.difficulty.AllowedBy(incidentDef.disabledWhen))
			{
				return false;
			}
			if (GenDate.DaysPassedSinceSettle < incidentDef.earliestDay)
			{
				return false;
			}
			if (incidentDef.Worker.FiredTooRecently(map))
			{
				return false;
			}
		}
		if (def == GameConditionDefOf.ColdSnap && !IncidentWorker_ColdSnap.IsTemperatureAppropriate(map))
		{
			return false;
		}
		if (def == GameConditionDefOf.HeatWave && !IncidentWorker_HeatWave.IsTemperatureAppropriate(map))
		{
			return false;
		}
		return true;
	}
}
