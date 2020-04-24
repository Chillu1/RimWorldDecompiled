using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetColonistCountFromColonyPercentage : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<float> colonyPercentage;

		public SlateRef<int> mustHaveFreeColonistsAvailableCount;

		protected override void RunInt()
		{
			SetVars(QuestGen.slate);
		}

		private void SetVars(Slate slate)
		{
			string value = storeAs.GetValue(slate);
			int num = PawnsFinder.AllMaps_FreeColonistsSpawned.Count((Pawn c) => !c.IsQuestLodger());
			int var = Mathf.Clamp((int)((float)num * colonyPercentage.GetValue(slate)), 1, num - 1);
			slate.Set(value, var);
		}

		protected override bool TestRunInt(Slate slate)
		{
			SetVars(slate);
			float num = mustHaveFreeColonistsAvailableCount.GetValue(slate);
			if (num > 0f)
			{
				return (float)PawnsFinder.AllMaps_FreeColonistsSpawned.Count((Pawn c) => !c.IsQuestLodger()) >= num;
			}
			return true;
		}
	}
}
