using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_ModIsActive : QuestNode
	{
		public SlateRef<string> modID;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (ModsConfig.IsActive(modID.GetValue(slate)))
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
			if (ModsConfig.IsActive(modID.GetValue(QuestGen.slate)))
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
