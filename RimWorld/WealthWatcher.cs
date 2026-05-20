using System.Collections.Generic;
using RimWorld.Planet;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace RimWorld;

public class WealthWatcher
{
	private Map map;

	private float wealthItems;

	private float wealthBuildings;

	private float wealthPawns;

	private float wealthFloorsOnly;

	private int totalHealth;

	private float lastCountTick = -99999f;

	private static float[] cachedTerrainMarketValue;

	private const int MinCountInterval = 5000;

	public const float WealthFactor_Slave = 0.75f;

	private List<Thing> tmpThings = new List<Thing>();

	public int HealthTotal
	{
		get
		{
			RecountIfNeeded();
			return totalHealth;
		}
	}

	public float WealthTotal
	{
		get
		{
			RecountIfNeeded();
			return wealthItems + wealthBuildings + wealthPawns;
		}
	}

	public float WealthItems
	{
		get
		{
			RecountIfNeeded();
			return wealthItems;
		}
	}

	public float WealthBuildings
	{
		get
		{
			RecountIfNeeded();
			return wealthBuildings;
		}
	}

	public float WealthFloorsOnly
	{
		get
		{
			RecountIfNeeded();
			return wealthFloorsOnly;
		}
	}

	public float WealthPawns
	{
		get
		{
			RecountIfNeeded();
			return wealthPawns;
		}
	}

	public static void ResetStaticData()
	{
		int num = -1;
		List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			num = Mathf.Max(num, allDefsListForReading[i].index);
		}
		cachedTerrainMarketValue = new float[num + 1];
		for (int j = 0; j < allDefsListForReading.Count; j++)
		{
			cachedTerrainMarketValue[allDefsListForReading[j].index] = allDefsListForReading[j].GetStatValueAbstract(StatDefOf.MarketValue);
		}
	}

	public WealthWatcher(Map map)
	{
		this.map = map;
	}

	private void RecountIfNeeded()
	{
		if (Current.ProgramState == ProgramState.Playing && (float)Find.TickManager.TicksGame - lastCountTick > 5000f)
		{
			ForceRecount();
		}
	}

	public void ForceRecount(bool allowDuringInit = false)
	{
		if (!allowDuringInit && Current.ProgramState != ProgramState.Playing)
		{
			Log.Error("WealthWatcher recount in game mode " + Current.ProgramState);
			return;
		}
		wealthItems = CalculateWealthItems();
		wealthBuildings = 0f;
		wealthPawns = 0f;
		wealthFloorsOnly = 0f;
		totalHealth = 0;
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (thing.Faction == Faction.OfPlayer)
			{
				wealthBuildings += thing.GetStatValue(StatDefOf.MarketValueIgnoreHp);
				totalHealth += thing.HitPoints;
			}
		}
		wealthFloorsOnly = CalculateWealthFloors();
		wealthBuildings += wealthFloorsOnly;
		foreach (Pawn item in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
		{
			if (!item.IsQuestLodger())
			{
				float num = item.MarketValue;
				if (item.IsSlave)
				{
					num *= 0.75f;
				}
				wealthPawns += num;
				if (item.IsFreeColonist)
				{
					totalHealth += Mathf.RoundToInt(item.health.summaryHealth.SummaryHealthPercent * 100f);
				}
			}
		}
		foreach (PocketMapParent pocketMap in Find.World.pocketMaps)
		{
			if (pocketMap.HasMap && pocketMap.sourceMap == map)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					pocketMap.Map.wealthWatcher.ForceRecount();
				}
				wealthItems += pocketMap.Map.wealthWatcher.wealthItems;
				wealthBuildings += pocketMap.Map.wealthWatcher.wealthBuildings;
				wealthPawns += pocketMap.Map.wealthWatcher.wealthPawns;
				wealthFloorsOnly += pocketMap.Map.wealthWatcher.wealthFloorsOnly;
				totalHealth += pocketMap.Map.wealthWatcher.totalHealth;
			}
		}
		lastCountTick = Find.TickManager.TicksGame;
	}

	public static float GetEquipmentApparelAndInventoryWealth(Pawn p)
	{
		float num = 0f;
		if (p.equipment != null)
		{
			List<ThingWithComps> allEquipmentListForReading = p.equipment.AllEquipmentListForReading;
			for (int i = 0; i < allEquipmentListForReading.Count; i++)
			{
				num += allEquipmentListForReading[i].MarketValue * (float)allEquipmentListForReading[i].stackCount;
			}
		}
		if (p.apparel != null)
		{
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int j = 0; j < wornApparel.Count; j++)
			{
				num += wornApparel[j].MarketValue * (float)wornApparel[j].stackCount;
			}
		}
		if (p.inventory != null)
		{
			ThingOwner<Thing> innerContainer = p.inventory.innerContainer;
			for (int k = 0; k < innerContainer.Count; k++)
			{
				num += innerContainer[k].MarketValue * (float)innerContainer[k].stackCount;
			}
		}
		return num;
	}

	private float CalculateWealthItems()
	{
		tmpThings.Clear();
		ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), tmpThings, allowUnreal: false, WealthItemsFilter);
		float num = 0f;
		for (int i = 0; i < tmpThings.Count; i++)
		{
			if (tmpThings[i].SpawnedOrAnyParentSpawned && !tmpThings[i].PositionHeld.Fogged(map))
			{
				num += tmpThings[i].MarketValue * (float)tmpThings[i].stackCount;
			}
		}
		tmpThings.Clear();
		return num;
	}

	public static bool WealthItemsFilter(IThingHolder x)
	{
		if (x is PassingShip || x is MapComponent)
		{
			return false;
		}
		Pawn pawn = x as Pawn;
		if (pawn != null && pawn.Faction != Faction.OfPlayer)
		{
			return false;
		}
		if (pawn != null && pawn.IsQuestLodger())
		{
			return false;
		}
		return true;
	}

	private float CalculateWealthFloors()
	{
		TerrainDef[] topGrid = map.terrainGrid.topGrid;
		TerrainDef[] foundationGrid = map.terrainGrid.foundationGrid;
		NativeBitArray fogGrid_Unsafe = map.fogGrid.FogGrid_Unsafe;
		IntVec3 size = map.Size;
		float num = 0f;
		if (!fogGrid_Unsafe.IsCreated)
		{
			return 0f;
		}
		int i = 0;
		for (int num2 = size.x * size.z; i < num2; i++)
		{
			if (!fogGrid_Unsafe.IsSet(i) && topGrid[i] != null)
			{
				num += cachedTerrainMarketValue[topGrid[i].index];
			}
			if (foundationGrid[i] != null)
			{
				num += cachedTerrainMarketValue[foundationGrid[i].index];
			}
		}
		return num;
	}
}
