using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_RemoveEquipmentFromPawns : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<Pawn>> pawns;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null)
		{
			QuestPart_RemoveEquipmentFromPawns questPart_RemoveEquipmentFromPawns = new QuestPart_RemoveEquipmentFromPawns();
			questPart_RemoveEquipmentFromPawns.pawns.AddRange(pawns.GetValue(slate));
			questPart_RemoveEquipmentFromPawns.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_RemoveEquipmentFromPawns);
		}
	}
}
