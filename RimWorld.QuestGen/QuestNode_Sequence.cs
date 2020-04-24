using System.Collections.Generic;

namespace RimWorld.QuestGen
{
	public class QuestNode_Sequence : QuestNode
	{
		public List<QuestNode> nodes = new List<QuestNode>();

		protected override void RunInt()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Run();
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (!nodes[i].TestRun(slate))
				{
					return false;
				}
			}
			return true;
		}
	}
}
