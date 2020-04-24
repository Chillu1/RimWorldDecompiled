using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetRandomByCurve : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<SimpleCurve> curve;

		public SlateRef<bool> roundRandom;

		public SlateRef<float?> min;

		public SlateRef<float?> max;

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
			float num = Rand.ByCurve(curve.GetValue(slate));
			if (roundRandom.GetValue(slate))
			{
				num = GenMath.RoundRandom(num);
			}
			if (min.GetValue(slate).HasValue)
			{
				num = Mathf.Max(num, min.GetValue(slate).Value);
			}
			if (max.GetValue(slate).HasValue)
			{
				num = Mathf.Min(num, max.GetValue(slate).Value);
			}
			slate.Set(storeAs.GetValue(slate), num);
		}
	}
}
