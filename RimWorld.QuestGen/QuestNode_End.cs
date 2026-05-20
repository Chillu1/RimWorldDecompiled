using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_End : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<QuestEndOutcome> outcome;

	public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

	public SlateRef<bool?> sendStandardLetter;

	public SlateRef<int> goodwillChangeAmount;

	public SlateRef<Thing> goodwillChangeFactionOf;

	public SlateRef<HistoryEventDef> goodwillChangeReason;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		int value = goodwillChangeAmount.GetValue(slate);
		Thing value2 = goodwillChangeFactionOf.GetValue(slate);
		if (value != 0 && value2 != null && value2.Faction != null)
		{
			QuestPart_FactionGoodwillChange questPart_FactionGoodwillChange = new QuestPart_FactionGoodwillChange();
			questPart_FactionGoodwillChange.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_FactionGoodwillChange.faction = value2.Faction;
			questPart_FactionGoodwillChange.change = value;
			questPart_FactionGoodwillChange.historyEvent = goodwillChangeReason.GetValue(slate);
			slate.Set("goodwillPenalty", Mathf.Abs(value).ToString());
			QuestGen.quest.AddPart(questPart_FactionGoodwillChange);
		}
		QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
		questPart_QuestEnd.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_QuestEnd.outcome = outcome.GetValue(slate);
		questPart_QuestEnd.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		questPart_QuestEnd.sendLetter = sendStandardLetter.GetValue(slate) == true;
		QuestGen.quest.AddPart(questPart_QuestEnd);
	}
}
