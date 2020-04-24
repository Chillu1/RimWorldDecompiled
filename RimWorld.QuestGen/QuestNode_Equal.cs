using System;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Equal : QuestNode
	{
		[NoTranslate]
		public SlateRef<object> value1;

		[NoTranslate]
		public SlateRef<object> value2;

		public SlateRef<Type> compareAs;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (QuestNodeEqualUtility.Equal(value1.GetValue(slate), value2.GetValue(slate), compareAs.GetValue(slate)))
			{
				if (node != null)
				{
					return node.TestRun(slate);
				}
				return true;
			}
			if (elseNode != null)
			{
				return elseNode.TestRun(slate);
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (QuestNodeEqualUtility.Equal(value1.GetValue(slate), value2.GetValue(slate), compareAs.GetValue(slate)))
			{
				if (node != null)
				{
					node.Run();
				}
			}
			else if (elseNode != null)
			{
				elseNode.Run();
			}
		}
	}
}
