using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_ChangeNeed : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<Pawn> pawn;

	public SlateRef<NeedDef> need;

	public SlateRef<float> offset;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_ChangeNeed questPart_ChangeNeed = new QuestPart_ChangeNeed();
		questPart_ChangeNeed.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_ChangeNeed.pawn = pawn.GetValue(slate);
		questPart_ChangeNeed.need = need.GetValue(slate);
		questPart_ChangeNeed.offset = offset.GetValue(slate);
		QuestGen.quest.AddPart(questPart_ChangeNeed);
	}
}
