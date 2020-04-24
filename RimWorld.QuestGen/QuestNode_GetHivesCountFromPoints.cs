using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetHivesCountFromPoints : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			SetVars(slate);
			return true;
		}

		protected override void RunInt()
		{
			SetVars(QuestGen.slate);
		}

		private void SetVars(Slate slate)
		{
			int num = Mathf.RoundToInt(slate.Get("points", 0f) / 220f);
			if (num < 1)
			{
				num = 1;
			}
			slate.Set(storeAs.GetValue(slate), num);
		}
	}
}
