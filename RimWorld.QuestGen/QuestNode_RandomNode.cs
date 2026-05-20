using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_RandomNode : QuestNode
{
	public List<QuestNode> nodes = new List<QuestNode>();

	protected override bool TestRunInt(Slate slate)
	{
		QuestNode questNode = GetNodesCanRun(slate).FirstOrDefault();
		if (questNode == null)
		{
			return false;
		}
		questNode.TestRun(slate);
		return true;
	}

	protected override void RunInt()
	{
		if (GetNodesCanRun(QuestGen.slate).TryRandomElementByWeight((QuestNode e) => e.SelectionWeight(QuestGen.slate), out var result))
		{
			result.Run();
		}
	}

	private IEnumerable<QuestNode> GetNodesCanRun(Slate slate)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].SelectionWeight(slate) > 0f && nodes[i].TestRun(slate.DeepCopy()))
			{
				yield return nodes[i];
			}
		}
	}
}
