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
			float num = slate.Get("points", 0f);
			num *= IncidentWorker_Infestation.PointsFactorCurve.Evaluate(num);
			int num2 = Mathf.RoundToInt(num / 220f);
			if (num2 < 1)
			{
				num2 = 1;
			}
			slate.Set(storeAs.GetValue(slate), num2);
		}
	}
}
