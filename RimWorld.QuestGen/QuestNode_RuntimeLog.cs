using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_RuntimeLog : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	[NoTranslate]
	public SlateRef<string> message;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_Log questPart_Log = new QuestPart_Log();
		questPart_Log.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Log.message = message.GetValue(slate);
		QuestGen.quest.AddPart(questPart_Log);
	}
}
