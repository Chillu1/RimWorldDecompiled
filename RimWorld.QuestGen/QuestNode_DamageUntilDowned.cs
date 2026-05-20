using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_DamageUntilDowned : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<bool?> allowBleedingWounds;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null)
		{
			QuestPart_DamageUntilDowned questPart_DamageUntilDowned = new QuestPart_DamageUntilDowned();
			questPart_DamageUntilDowned.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_DamageUntilDowned.pawns.AddRange(pawns.GetValue(slate));
			questPart_DamageUntilDowned.allowBleedingWounds = allowBleedingWounds.GetValue(slate) ?? true;
			QuestGen.quest.AddPart(questPart_DamageUntilDowned);
		}
	}
}
