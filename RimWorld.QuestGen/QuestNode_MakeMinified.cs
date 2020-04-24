using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_MakeMinified : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<Thing> thing;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			MinifiedThing var = thing.GetValue(slate).MakeMinified();
			QuestGen.slate.Set(storeAs.GetValue(slate), var);
		}
	}
}
