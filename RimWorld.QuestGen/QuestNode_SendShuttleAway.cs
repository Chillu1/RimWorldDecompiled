using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SendShuttleAway : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

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
			QuestPart_SendShuttleAway questPart_SendShuttleAway = new QuestPart_SendShuttleAway();
			questPart_SendShuttleAway.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SendShuttleAway.shuttle = shuttle.GetValue(slate);
			questPart_SendShuttleAway.dropEverything = dropEverything.GetValue(slate);
			QuestGen.quest.AddPart(questPart_SendShuttleAway);
		}
	}
}
