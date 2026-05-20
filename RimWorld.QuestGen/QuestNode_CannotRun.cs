namespace RimWorld.QuestGen
{
	public class QuestNode_CannotRun : QuestNode
	{
		protected override void RunInt()
		{
		}

		protected override bool TestRunInt(Slate slate)
		{
			return false;
		}
	}
}
