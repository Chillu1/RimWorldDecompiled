namespace RimWorld.QuestGen
{
	public class QuestNode_FactionExists : QuestNode
	{
		public SlateRef<Faction> faction;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (FactionExists(slate))
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
			if (FactionExists(QuestGen.slate))
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

		private bool FactionExists(Slate slate)
		{
			return faction.GetValue(slate) != null;
		}
	}
}
