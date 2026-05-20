using UnityEngine;
using Verse;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class SettleUtility
{
	public static readonly Texture2D SettleCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Settle");

	public static readonly Texture2D CreateCampCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CreateCamp");

	public static bool PlayerSettlementsCountLimitReached
	{
		get
		{
			int num = 0;
			foreach (Map map in Find.Maps)
			{
				if (map.IsPlayerHome && map.Parent is Settlement)
				{
					num++;
				}
				else if (map.wasSpawnedViaGravShipLanding)
				{
					num++;
				}
			}
			return num >= Prefs.MaxNumberOfPlayerSettlements;
		}
	}

	public static Settlement AddNewHome(PlanetTile tile, Faction faction)
	{
		AbandonedArchotechStructures abandonedArchotechStructures = Find.WorldObjects.WorldObjectAt<AbandonedArchotechStructures>(tile);
		Settlement settlement = ((abandonedArchotechStructures == null) ? ((Settlement)WorldObjectMaker.MakeWorldObject(tile.LayerDef.SettlementWorldObjectDef)) : abandonedArchotechStructures.GenerateSettlementAndDestroy());
		settlement.Tile = tile;
		settlement.SetFaction(faction);
		settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
		Find.WorldObjects.Add(settlement);
		if (faction == Faction.OfPlayer)
		{
			TaleRecorder.RecordTale(TaleDefOf.TileSettled).customLabel = "NewSettlement".Translate();
			if (Find.IdeoManager != null)
			{
				Find.IdeoManager.lastResettledTick = GenTicks.TicksGame;
			}
		}
		return settlement;
	}
}
