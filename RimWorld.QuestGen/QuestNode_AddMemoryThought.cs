using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddMemoryThought : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<ThoughtDef> def;

	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<Pawn> otherPawn;

	public SlateRef<bool?> addToLookTargets;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) == null)
		{
			return;
		}
		foreach (Pawn item in pawns.GetValue(slate))
		{
			QuestPart_AddMemoryThought questPart_AddMemoryThought = new QuestPart_AddMemoryThought();
			questPart_AddMemoryThought.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_AddMemoryThought.def = def.GetValue(slate);
			questPart_AddMemoryThought.pawn = item;
			questPart_AddMemoryThought.otherPawn = otherPawn.GetValue(slate);
			questPart_AddMemoryThought.addToLookTargets = addToLookTargets.GetValue(slate) ?? true;
			QuestGen.quest.AddPart(questPart_AddMemoryThought);
		}
	}
}
