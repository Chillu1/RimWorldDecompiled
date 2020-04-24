using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_EvaluateSimpleCurve : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<SimpleCurve> curve;

		public SlateRef<float> value;

		public SlateRef<bool> roundRandom;

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
			float num = curve.GetValue(slate).Evaluate(value.GetValue(slate));
			if (roundRandom.GetValue(slate))
			{
				num = GenMath.RoundRandom(num);
			}
			slate.Set(storeAs.GetValue(slate), num);
		}
	}
}
