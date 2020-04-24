using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_MultiplyRange : QuestNode
	{
		public SlateRef<FloatRange> range;

		public SlateRef<float> value;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			return !storeAs.GetValue(slate).NullOrEmpty();
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			slate.Set(storeAs.GetValue(slate), range.GetValue(slate) * value.GetValue(slate));
		}
	}
}
