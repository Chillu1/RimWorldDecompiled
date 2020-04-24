using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetMapOf : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs = "map";

		public SlateRef<Thing> mapOf;

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
			if (mapOf.GetValue(slate) != null)
			{
				slate.Set(storeAs.GetValue(slate), mapOf.GetValue(slate).MapHeld);
			}
		}
	}
}
