using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_RemoveMemoryThought : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<ThoughtDef> def;

	public SlateRef<Pawn> pawn;

	public SlateRef<Pawn> otherPawn;

	public SlateRef<int?> count;

	public SlateRef<bool?> addToLookTargets;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_RemoveMemoryThought questPart_RemoveMemoryThought = new QuestPart_RemoveMemoryThought();
		questPart_RemoveMemoryThought.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_RemoveMemoryThought.def = def.GetValue(slate);
		questPart_RemoveMemoryThought.pawn = pawn.GetValue(slate);
		questPart_RemoveMemoryThought.count = count.GetValue(slate);
		questPart_RemoveMemoryThought.otherPawn = otherPawn.GetValue(slate);
		questPart_RemoveMemoryThought.addToLookTargets = addToLookTargets.GetValue(slate) ?? true;
		QuestGen.quest.AddPart(questPart_RemoveMemoryThought);
	}
}
