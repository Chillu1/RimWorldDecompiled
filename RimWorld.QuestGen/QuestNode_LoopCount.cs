using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_LoopCount : QuestNode
	{
		public QuestNode node;

		public SlateRef<int> loopCount;

		[NoTranslate]
		public SlateRef<string> storeLoopCounterAs;

		protected override bool TestRunInt(Slate slate)
		{
			for (int i = 0; i < loopCount.GetValue(slate); i++)
			{
				if (storeLoopCounterAs.GetValue(slate) != null)
				{
					slate.Set(storeLoopCounterAs.GetValue(slate), i);
				}
				try
				{
					if (!node.TestRun(slate))
					{
						return false;
					}
				}
				finally
				{
					slate.PopPrefix();
				}
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			for (int i = 0; i < loopCount.GetValue(slate); i++)
			{
				if (storeLoopCounterAs.GetValue(slate) != null)
				{
					QuestGen.slate.Set(storeLoopCounterAs.GetValue(slate), i);
				}
				try
				{
					node.Run();
				}
				finally
				{
					QuestGen.slate.PopPrefix();
				}
			}
		}
	}
}
