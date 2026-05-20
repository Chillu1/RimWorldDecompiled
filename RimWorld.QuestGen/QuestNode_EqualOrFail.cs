using System;

namespace RimWorld.QuestGen;

public class QuestNode_EqualOrFail : QuestNode
{
	public SlateRef<object> value1;

	public SlateRef<object> value2;

	public SlateRef<Type> compareAs;

	public QuestNode node;

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
		return false;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (QuestNodeEqualUtility.Equal(value1.GetValue(slate), value2.GetValue(slate), compareAs.GetValue(slate)) && node != null)
		{
			node.Run();
		}
	}
}
