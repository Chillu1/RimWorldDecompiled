using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Unset : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> name;

		protected override bool TestRunInt(Slate slate)
		{
			slate.Remove(name.GetValue(slate));
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.slate.Remove(name.GetValue(slate));
		}
	}
}
