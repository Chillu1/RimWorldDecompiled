using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_TradeRequest_Initiate : QuestNode
{
	public SlateRef<Settlement> settlement;

	public SlateRef<ThingDef> requestedThingDef;

	public SlateRef<int> requestedThingCount;

	public SlateRef<int> duration;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_InitiateTradeRequest part = new QuestPart_InitiateTradeRequest
		{
			settlement = settlement.GetValue(slate),
			requestedThingDef = requestedThingDef.GetValue(slate),
			requestedCount = requestedThingCount.GetValue(slate),
			requestDuration = duration.GetValue(slate),
			keepAfterQuestEnds = false,
			inSignal = slate.Get<string>("inSignal")
		};
		QuestGen.quest.AddPart(part);
	}

	protected override bool TestRunInt(Slate slate)
	{
		return settlement.GetValue(slate) != null && requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) != null && duration.GetValue(slate) > 0;
	}
}
