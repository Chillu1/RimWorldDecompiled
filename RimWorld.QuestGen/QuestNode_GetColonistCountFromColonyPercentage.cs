using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetColonistCountFromColonyPercentage : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<float> colonyPercentage;

	public SlateRef<int> mustHaveFreeColonistsAvailableCount;

	public SlateRef<float?> minAge;

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private void SetVars(Slate slate)
	{
		string value = storeAs.GetValue(slate);
		float minAgeResolved = minAge.GetValue(slate).GetValueOrDefault();
		int num = PawnsFinder.AllMaps_FreeColonistsSpawned.Count((Pawn c) => ColonistCounts(c, minAgeResolved));
		int var = Mathf.Clamp((int)((float)num * colonyPercentage.GetValue(slate)), 1, num - 1);
		slate.Set(value, var);
	}

	protected override bool TestRunInt(Slate slate)
	{
		SetVars(slate);
		float num = mustHaveFreeColonistsAvailableCount.GetValue(slate);
		if (num > 0f)
		{
			float minAgeResolved = minAge.GetValue(slate).GetValueOrDefault();
			return (float)PawnsFinder.AllMaps_FreeColonistsSpawned.Count((Pawn c) => ColonistCounts(c, minAgeResolved)) >= num;
		}
		return true;
	}

	private bool ColonistCounts(Pawn pawn, float minAge)
	{
		if (pawn.IsQuestLodger())
		{
			return false;
		}
		if (pawn.ageTracker.AgeBiologicalYearsFloat < minAge)
		{
			return false;
		}
		return true;
	}
}
