namespace RimWorld.QuestGen;

public class QuestNode_ReplaceLostLeaderReferences : QuestNode
{
	public SlateRef<string> inSignal;

	public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_ReplaceLostLeaderReferences questPart_ReplaceLostLeaderReferences = new QuestPart_ReplaceLostLeaderReferences();
		questPart_ReplaceLostLeaderReferences.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
		questPart_ReplaceLostLeaderReferences.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		QuestGen.quest.AddPart(questPart_ReplaceLostLeaderReferences);
	}
}
