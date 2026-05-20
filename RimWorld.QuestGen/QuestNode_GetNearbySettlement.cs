using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetNearbySettlement : QuestNode
{
	public SlateRef<bool> allowActiveTradeRequest = true;

	public SlateRef<bool> canBeSpace;

	public SlateRef<bool> requireSameOrAdjacentLayer;

	public SlateRef<float> maxTileDistance;

	[NoTranslate]
	public SlateRef<string> storeAs;

	[NoTranslate]
	public SlateRef<string> storeFactionAs;

	[NoTranslate]
	public SlateRef<string> storeFactionLeaderAs;

	[NoTranslate]
	public SlateRef<string> storeCanCaravanAs;

	public SlateRef<List<PlanetLayerDef>> layerWhitelist;

	public SlateRef<List<PlanetLayerDef>> layerBlacklist;

	private Settlement RandomNearbyTradeableSettlement(PlanetTile originTile, Slate slate)
	{
		return Find.WorldObjects.SettlementBases.Where(Validator).RandomElementWithFallback();
		bool Validator(Settlement settlement)
		{
			if (!settlement.Visitable)
			{
				return false;
			}
			if (!canBeSpace.GetValue(slate) && settlement.Tile.LayerDef.isSpace)
			{
				return false;
			}
			List<PlanetLayerDef> value = layerWhitelist.GetValue(slate);
			List<PlanetLayerDef> value2 = layerBlacklist.GetValue(slate);
			if (!value.NullOrEmpty() && settlement.Tile.Valid && !value.Contains(settlement.Tile.LayerDef))
			{
				return false;
			}
			if (!value2.NullOrEmpty() && settlement.Tile.Valid && value2.Contains(settlement.Tile.LayerDef))
			{
				return false;
			}
			if (requireSameOrAdjacentLayer.GetValue(slate) && settlement.Tile.Valid && originTile.Valid && settlement.Tile.Layer != originTile.Layer && !settlement.Tile.Layer.DirectConnectionTo(originTile.Layer))
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
							if (partsListForReading[j] is QuestPart_InitiateTradeRequest questPart_InitiateTradeRequest && questPart_InitiateTradeRequest.settlement == settlement)
							{
								return false;
							}
						}
					}
				}
			}
			if (GravshipUtility.PlayerHasGravEngine())
			{
				return true;
			}
			if (Find.WorldReachability.CanReach(originTile, settlement.Tile))
			{
				return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < maxTileDistance.GetValue(slate);
			}
			return false;
		}
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
		if (!storeCanCaravanAs.GetValue(slate).NullOrEmpty())
		{
			bool var = settlement.Tile.Valid && map.Tile.Valid && settlement.Tile.Layer == map.Tile.Layer && settlement.Tile.LayerDef.SurfaceTiles;
			QuestGen.slate.Set(storeCanCaravanAs.GetValue(slate), var);
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		Map map = slate.Get<Map>("map");
		if (map == null)
		{
			return false;
		}
		Settlement settlement = RandomNearbyTradeableSettlement(map.Tile, slate);
		if (settlement != null)
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
