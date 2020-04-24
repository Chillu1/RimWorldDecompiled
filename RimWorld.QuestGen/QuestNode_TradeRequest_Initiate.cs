using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_TradeRequest_Initiate : QuestNode
	{
		public SlateRef<Settlement> settlement;

		public SlateRef<ThingDef> requestedThingDef;

		public SlateRef<int> requestedThingCount;

		public SlateRef<int> duration;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_InitiateTradeRequest questPart_InitiateTradeRequest = new QuestPart_InitiateTradeRequest();
			questPart_InitiateTradeRequest.settlement = settlement.GetValue(slate);
			questPart_InitiateTradeRequest.requestedThingDef = requestedThingDef.GetValue(slate);
			questPart_InitiateTradeRequest.requestedCount = requestedThingCount.GetValue(slate);
			questPart_InitiateTradeRequest.requestDuration = duration.GetValue(slate);
			questPart_InitiateTradeRequest.keepAfterQuestEnds = false;
			questPart_InitiateTradeRequest.inSignal = slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_InitiateTradeRequest);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return settlement.GetValue(slate) != null && requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) != null && duration.GetValue(slate) > 0;
		}
	}
}
