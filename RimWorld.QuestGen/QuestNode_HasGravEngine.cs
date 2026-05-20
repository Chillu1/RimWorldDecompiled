namespace RimWorld.QuestGen;

public class QuestNode_HasGravEngine : QuestNode
{
	public QuestNode node;

	public QuestNode elseNode;

	protected override bool TestRunInt(Slate slate)
	{
		if (GravshipUtility.PlayerHasGravEngine())
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
		if (GravshipUtility.PlayerHasGravEngine())
		{
			node?.Run();
		}
		else
		{
			elseNode?.Run();
		}
	}
}
