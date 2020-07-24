using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetNearbySettlement : QuestNode
	{
		public SlateRef<bool> allowActiveTradeRequest = true;

		public SlateRef<float> maxTileDistance;

		[NoTranslate]
		public SlateRef<string> storeAs;

		[NoTranslate]
		public SlateRef<string> storeFactionAs;

		[NoTranslate]
		public SlateRef<string> storeFactionLeaderAs;

		private Settlement RandomNearbyTradeableSettlement(int originTile, Slate slate)
		{
			return Find.WorldObjects.SettlementBases.Where(delegate(Settlement settlement)
			{
				if (!settlement.Visitable)
				{
					return false;
				}
				if (!allowActiveTradeRequest.GetValue(slate))
				{
					if (settlement.GetComponent<TradeRequestComp>() != null && settlement.GetComponent<TradeRequestComp>().ActiveRequest)
					{
						return false;
					}
					List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
					for (int i = 0; i < questsListForReading.Count; i++)
					{
						if (!questsListForReading[i].Historical)
						{
							List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
							for (int j = 0; j < partsListForReading.Count; j++)
							{
								QuestPart_InitiateTradeRequest questPart_InitiateTradeRequest;
								if ((questPart_InitiateTradeRequest = (partsListForReading[j] as QuestPart_InitiateTradeRequest)) != null && questPart_InitiateTradeRequest.settlement == settlement)
								{
									return false;
								}
							}
						}
					}
				}
				return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < maxTileDistance.GetValue(slate) && Find.WorldReachability.CanReach(originTile, settlement.Tile);
			}).RandomElementWithFallback();
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Map map = QuestGen.slate.Get<Map>("map");
			Settlement settlement = RandomNearbyTradeableSettlement(map.Tile, slate);
			QuestGen.slate.Set(storeAs.GetValue(slate), settlement);
			if (!string.IsNullOrEmpty(storeFactionAs.GetValue(slate)))
			{
				QuestGen.slate.Set(storeFactionAs.GetValue(slate), settlement.Faction);
			}
			if (!storeFactionLeaderAs.GetValue(slate).NullOrEmpty())
			{
				QuestGen.slate.Set(storeFactionLeaderAs.GetValue(slate), settlement.Faction.leader);
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			Map map = slate.Get<Map>("map");
			Settlement settlement = RandomNearbyTradeableSettlement(map.Tile, slate);
			if (map != null && settlement != null)
			{
				slate.Set(storeAs.GetValue(slate), settlement);
				if (!string.IsNullOrEmpty(storeFactionAs.GetValue(slate)))
				{
					slate.Set(storeFactionAs.GetValue(slate), settlement.Faction);
				}
				if (!string.IsNullOrEmpty(storeFactionLeaderAs.GetValue(slate)))
				{
					slate.Set(storeFactionLeaderAs.GetValue(slate), settlement.Faction.leader);
				}
				return true;
			}
			return false;
		}
	}
}
