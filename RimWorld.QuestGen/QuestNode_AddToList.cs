using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddToList : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> name;

		public SlateRef<object> value;

		protected override bool TestRunInt(Slate slate)
		{
			QuestGenUtility.AddToOrMakeList(slate, name.GetValue(slate), value.GetValue(slate));
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGenUtility.AddToOrMakeList(QuestGen.slate, name.GetValue(slate), value.GetValue(slate));
		}
	}
}
