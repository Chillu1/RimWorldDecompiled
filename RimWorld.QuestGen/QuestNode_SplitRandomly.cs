using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SplitRandomly : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAsFormat;

		[NoTranslate]
		public SlateRef<string> storeAs1;

		[NoTranslate]
		public SlateRef<string> storeAs2;

		[NoTranslate]
		public SlateRef<string> storeAs3;

		[NoTranslate]
		public SlateRef<string> storeAs4;

		[NoTranslate]
		public SlateRef<string> storeAs5;

		public SlateRef<float?> valueToSplit;

		public SlateRef<int> countToSplit;

		public SlateRef<float> zeroIfFractionBelow1;

		public SlateRef<float> zeroIfFractionBelow2;

		public SlateRef<float> zeroIfFractionBelow3;

		public SlateRef<float> zeroIfFractionBelow4;

		public SlateRef<float> zeroIfFractionBelow5;

		public SlateRef<float> minFraction1;

		public SlateRef<float> minFraction2;

		public SlateRef<float> minFraction3;

		public SlateRef<float> minFraction4;

		public SlateRef<float> minFraction5;

		private static List<float> tmpValues = new List<float>();

		private static List<float> tmpZeroIfFractionBelow = new List<float>();

		private static List<float> tmpMinFractions = new List<float>();

		protected override bool TestRunInt(Slate slate)
		{
			DoWork(slate);
			return true;
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate);
		}

		private void DoWork(Slate slate)
		{
			float num = valueToSplit.GetValue(slate) ?? 1f;
			tmpMinFractions.Clear();
			tmpMinFractions.Add(minFraction1.GetValue(slate));
			tmpMinFractions.Add(minFraction2.GetValue(slate));
			tmpMinFractions.Add(minFraction3.GetValue(slate));
			tmpMinFractions.Add(minFraction4.GetValue(slate));
			tmpMinFractions.Add(minFraction5.GetValue(slate));
			tmpZeroIfFractionBelow.Clear();
			tmpZeroIfFractionBelow.Add(zeroIfFractionBelow1.GetValue(slate));
			tmpZeroIfFractionBelow.Add(zeroIfFractionBelow2.GetValue(slate));
			tmpZeroIfFractionBelow.Add(zeroIfFractionBelow3.GetValue(slate));
			tmpZeroIfFractionBelow.Add(zeroIfFractionBelow4.GetValue(slate));
			tmpZeroIfFractionBelow.Add(zeroIfFractionBelow5.GetValue(slate));
			Rand.SplitRandomly(num, countToSplit.GetValue(slate), tmpValues, tmpZeroIfFractionBelow, tmpMinFractions);
			for (int i = 0; i < tmpValues.Count; i++)
			{
				if (storeAsFormat.GetValue(slate) != null)
				{
					slate.Set(storeAsFormat.GetValue(slate).Formatted(i.Named("INDEX")), tmpValues[i]);
				}
				if (i == 0 && storeAs1.GetValue(slate) != null)
				{
					slate.Set(storeAs1.GetValue(slate), tmpValues[i]);
				}
				else if (i == 1 && storeAs2.GetValue(slate) != null)
				{
					slate.Set(storeAs2.GetValue(slate), tmpValues[i]);
				}
				else if (i == 2 && storeAs3.GetValue(slate) != null)
				{
					slate.Set(storeAs3.GetValue(slate), tmpValues[i]);
				}
				else if (i == 3 && storeAs4.GetValue(slate) != null)
				{
					slate.Set(storeAs4.GetValue(slate), tmpValues[i]);
				}
				else if (i == 4 && storeAs5.GetValue(slate) != null)
				{
					slate.Set(storeAs5.GetValue(slate), tmpValues[i]);
				}
			}
		}
	}
}
