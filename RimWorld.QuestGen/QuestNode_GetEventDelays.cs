using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetEventDelays : QuestNode
	{
		public SlateRef<int> durationTicks;

		public SlateRef<IntRange> intervalTicksRange;

		[NoTranslate]
		public SlateRef<string> storeCountAs;

		[NoTranslate]
		public SlateRef<string> storeDelaysAs;

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
			if (intervalTicksRange.GetValue(slate).max <= 0)
			{
				Log.Error("intervalTicksRange with max <= 0");
				return;
			}
			int num = 0;
			int num2 = 0;
			while (true)
			{
				num += intervalTicksRange.GetValue(slate).RandomInRange;
				if (num > durationTicks.GetValue(slate))
				{
					break;
				}
				slate.Set(storeDelaysAs.GetValue(slate).Formatted(num2.Named("INDEX")), num);
				num2++;
			}
			slate.Set(storeCountAs.GetValue(slate), num2);
		}
	}
}
