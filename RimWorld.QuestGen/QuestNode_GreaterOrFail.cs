namespace RimWorld.QuestGen;

public class QuestNode_GreaterOrFail : QuestNode
{
	public SlateRef<double> value1;

	public SlateRef<double> value2;

	public QuestNode node;

	protected override bool TestRunInt(Slate slate)
	{
		if (value1.GetValue(slate) > value2.GetValue(slate))
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
		if (value1.GetValue(slate) > value2.GetValue(slate) && node != null)
		{
			node.Run();
		}
	}
}
