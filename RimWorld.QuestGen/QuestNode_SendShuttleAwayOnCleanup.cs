using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SendShuttleAwayOnCleanup : QuestNode
	{
		public SlateRef<Thing> shuttle;

		public SlateRef<bool> dropEverything;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (shuttle.GetValue(slate) != null)
			{
				QuestPart_SendShuttleAwayOnCleanup questPart_SendShuttleAwayOnCleanup = new QuestPart_SendShuttleAwayOnCleanup();
				questPart_SendShuttleAwayOnCleanup.shuttle = shuttle.GetValue(slate);
				questPart_SendShuttleAwayOnCleanup.dropEverything = dropEverything.GetValue(slate);
				QuestGen.quest.AddPart(questPart_SendShuttleAwayOnCleanup);
			}
		}
	}
}
