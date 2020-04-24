namespace RimWorld.QuestGen
{
	public class QuestNode_RequirementsToAcceptColonistWithTitle : QuestNode
	{
		public SlateRef<RoyalTitleDef> minimumTitle;

		public SlateRef<Faction> faction;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.quest.AddPart(new QuestPart_RequirementsToAcceptColonistWithTitle
			{
				minimumTitle = minimumTitle.GetValue(slate),
				faction = faction.GetValue(slate)
			});
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
