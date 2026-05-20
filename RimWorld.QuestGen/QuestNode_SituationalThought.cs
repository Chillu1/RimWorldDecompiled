using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SituationalThought : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	public SlateRef<ThoughtDef> def;

	public SlateRef<Pawn> pawn;

	public SlateRef<int> delayTicks;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_SituationalThought questPart_SituationalThought = new QuestPart_SituationalThought();
		questPart_SituationalThought.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_SituationalThought.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		questPart_SituationalThought.def = def.GetValue(slate);
		questPart_SituationalThought.pawn = pawn.GetValue(slate);
		questPart_SituationalThought.delayTicks = delayTicks.GetValue(slate);
		QuestGen.quest.AddPart(questPart_SituationalThought);
	}
}
