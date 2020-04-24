using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetRandomInRangeFloat : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<FloatRange> range;

		protected override bool TestRunInt(Slate slate)
		{
			slate.Set(storeAs.GetValue(slate), range.GetValue(slate).RandomInRange);
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.slate.Set(storeAs.GetValue(slate), range.GetValue(slate).RandomInRange);
		}
	}
}
