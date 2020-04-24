namespace RimWorld.QuestGen
{
	public class QuestNode_SetTicksUntilAcceptanceExpiry : QuestNode
	{
		public SlateRef<int> ticks;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.quest.ticksUntilAcceptanceExpiry = ticks.GetValue(slate);
		}
	}
}
