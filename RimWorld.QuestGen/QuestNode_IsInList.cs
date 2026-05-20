using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_IsInList : QuestNode
{
	[NoTranslate]
	public SlateRef<string> name;

	public SlateRef<object> value;

	public QuestNode node;

	public QuestNode elseNode;

	protected override bool TestRunInt(Slate slate)
	{
		if (QuestGenUtility.IsInList(slate, name.GetValue(slate), value.GetValue(slate)))
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
		if (QuestGenUtility.IsInList(slate, name.GetValue(slate), value.GetValue(slate)))
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
