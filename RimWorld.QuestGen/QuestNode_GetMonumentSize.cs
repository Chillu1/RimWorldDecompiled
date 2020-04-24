using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetMonumentSize : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<MonumentMarker> monumentMarker;

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
			if (monumentMarker.GetValue(slate) != null)
			{
				slate.Set(storeAs.GetValue(slate), monumentMarker.GetValue(slate).Size);
			}
		}
	}
}
