using System;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Clamp : QuestNode
	{
		public SlateRef<double> value;

		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<double?> min;

		public SlateRef<double?> max;

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
			double num = value.GetValue(slate);
			if (min.GetValue(slate).HasValue)
			{
				num = Math.Max(num, min.GetValue(slate).Value);
			}
			if (max.GetValue(slate).HasValue)
			{
				num = Math.Min(num, max.GetValue(slate).Value);
			}
			slate.Set(storeAs.GetValue(slate), num);
		}
	}
}
