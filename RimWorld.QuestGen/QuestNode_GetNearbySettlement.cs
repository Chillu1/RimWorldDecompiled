using RimWorld.Planet;
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
				if (!settlement.Visitable || (!allowActiveTradeRequest.GetValue(slate) && settlement.GetComponent<TradeRequestComp>() != null && settlement.GetComponent<TradeRequestComp>().ActiveRequest))
				{
					return false;
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
