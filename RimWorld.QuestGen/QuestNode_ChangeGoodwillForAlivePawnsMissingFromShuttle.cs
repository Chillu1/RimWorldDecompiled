using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_ChangeGoodwillForAlivePawnsMissingFromShuttle : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<List<Pawn>> pawns;

	public SlateRef<Faction> faction;

	public SlateRef<int> goodwillChange;

	public SlateRef<HistoryEventDef> reason;

	protected override bool TestRunInt(Slate slate)
	{
		if (faction.GetValue(slate) == null)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_ChangeGoodwillForAlivePawnsMissingFromShuttle part = new QuestPart_ChangeGoodwillForAlivePawnsMissingFromShuttle
		{
			inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal")),
			pawns = pawns.GetValue(slate),
			faction = faction.GetValue(slate),
			goodwillChange = goodwillChange.GetValue(slate),
			historyEvent = reason.GetValue(slate)
		};
		QuestGen.quest.AddPart(part);
	}
}
