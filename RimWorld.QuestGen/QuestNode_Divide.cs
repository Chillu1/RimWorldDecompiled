using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Divide : QuestNode
	{
		public SlateRef<double> value1;

		public SlateRef<double> value2;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			return !storeAs.GetValue(slate).NullOrEmpty();
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			slate.Set(storeAs.GetValue(slate), value1.GetValue(slate) / value2.GetValue(slate));
		}
	}
}
