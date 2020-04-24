namespace RimWorld.QuestGen
{
	public class QuestNode_IsTrue : QuestNode
	{
		public SlateRef<bool> value;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (value.GetValue(slate))
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
			if (value.GetValue(slate))
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
