using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Set : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> name;

		public SlateRef<object> value;

		protected override bool TestRunInt(Slate slate)
		{
			slate.Set(name.GetValue(slate), value.GetValue(slate));
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.slate.Set(name.GetValue(slate), value.GetValue(slate));
		}
	}
}
