using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Chance : QuestNode
	{
		public SlateRef<float> chance;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (node == null || elseNode == null)
			{
				return true;
			}
			if (node.TestRun(slate.DeepCopy()))
			{
				node.TestRun(slate);
				return true;
			}
			if (elseNode.TestRun(slate.DeepCopy()))
			{
				elseNode.TestRun(slate);
				return true;
			}
			return false;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (Rand.Chance(chance.GetValue(slate)))
			{
				if (node != null)
				{
					if (node.TestRun(QuestGen.slate.DeepCopy()))
					{
						node.Run();
					}
					else if (elseNode != null && elseNode.TestRun(QuestGen.slate.DeepCopy()))
					{
						elseNode.Run();
					}
				}
			}
			else if (elseNode != null)
			{
				if (elseNode.TestRun(QuestGen.slate.DeepCopy()))
				{
					elseNode.Run();
				}
				else if (node != null && node.TestRun(QuestGen.slate.DeepCopy()))
				{
					node.Run();
				}
			}
		}
	}
}
