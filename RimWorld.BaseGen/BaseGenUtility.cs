using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public static class BaseGenUtility
{
	public const int MaxCheapWallStuffMarketValuePerUnit = 5;

	private static List<TerrainDef> tmpFactionFloors = new List<TerrainDef>();

	private static List<IntVec3> bridgeCells = new List<IntVec3>();

	public static ThingDef CheapStuffFor(ThingDef thingDef, Faction faction)
	{
		ThingDef thingDef2 = (from stuff in GenStuff.AllowedStuffsFor(thingDef, faction?.def.techLevel ?? TechLevel.Undefined, checkAllowedInStuffGeneration: true)
			where stuff.BaseMarketValue / stuff.VolumePerUnit < 5f && stuff.stuffProps.categories.Contains(StuffCategoryDefOf.Stony)
			select stuff).RandomElementWithFallback();
		if (thingDef2 != null)
		{
			return thingDef2;
		}
		if (ThingDefOf.WoodLog.stuffProps.CanMake(thingDef))
		{
			return ThingDefOf.WoodLog;
		}
		return GenStuff.RandomStuffInexpensiveFor(thingDef, faction);
	}

	public static ThingDef RandomCheapWallStuff(Faction faction, bool notVeryFlammable = false)
	{
		return RandomCheapWallStuff(faction?.def.techLevel ?? TechLevel.Spacer, notVeryFlammable);
	}

	public static ThingDef RandomCheapWallStuff(TechLevel techLevel, bool notVeryFlammable = false)
	{
		if (techLevel.IsNeolithicOrWorse())
		{
			return ThingDefOf.WoodLog;
		}
		ThingDef thingDef = (from d in GenStuff.AllowedStuffsFor(ThingDefOf.Wall, techLevel, checkAllowedInStuffGeneration: true)
			where d.BaseMarketValue / d.VolumePerUnit < 5f && d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony)
			select d).RandomElementWithFallback();
		if (thingDef != null)
		{
			return thingDef;
		}
		return (from d in GenStuff.AllowedStuffsFor(ThingDefOf.Wall, techLevel, checkAllowedInStuffGeneration: true)
			where d.BaseMarketValue / d.VolumePerUnit < 5f && (!notVeryFlammable || d.BaseFlammability < 0.5f)
			select d).RandomElement();
	}

	public static ThingDef RandomHightechWallStuff()
	{
		if (Rand.Value < 0.15f)
		{
			return ThingDefOf.Plasteel;
		}
		return ThingDefOf.Steel;
	}

	private static bool BuildCostsAllowedInStuffGen(BuildableDef buildableDef)
	{
		if (buildableDef.costList.NullOrEmpty())
		{
			return true;
		}
		for (int i = 0; i < buildableDef.costList.Count; i++)
		{
			ThingDef thingDef = buildableDef.costList[i].thingDef;
			if (thingDef != null && thingDef.IsStuff && !thingDef.stuffProps.allowedInStuffGeneration)
			{
				return false;
			}
		}
		return true;
	}

	public static TerrainDef RandomHightechFloorDef()
	{
		return Rand.Element(TerrainDefOf.AncientConcrete, TerrainDefOf.PavedTile, TerrainDefOf.AncientTile);
	}

	private static IEnumerable<TerrainDef> IdeoFloorTypes(Faction faction, bool allowCarpet = true)
	{
		if (faction == null || faction.ideos == null)
		{
			yield break;
		}
		foreach (Ideo allIdeo in faction.ideos.AllIdeos)
		{
			foreach (BuildableDef cachedPossibleBuildable in allIdeo.cachedPossibleBuildables)
			{
				if (cachedPossibleBuildable is TerrainDef terrainDef && BuildCostsAllowedInStuffGen(terrainDef) && (allowCarpet || !terrainDef.IsCarpet))
				{
					yield return terrainDef;
				}
			}
		}
	}

	public static TerrainDef RandomBasicFloorDef(Faction faction, bool allowCarpet = false)
	{
		bool flag = allowCarpet && (faction == null || !faction.def.techLevel.IsNeolithicOrWorse()) && Rand.Chance(0.1f);
		if (faction != null && faction.ideos != null && IdeoFloorTypes(faction, flag).TryRandomElement(out var result))
		{
			return result;
		}
		if (flag)
		{
			return DefDatabase<TerrainDef>.AllDefsListForReading.Where((TerrainDef x) => x.IsCarpet).RandomElement();
		}
		return Rand.Element(TerrainDefOf.AncientTile, TerrainDefOf.PavedTile, TerrainDefOf.WoodPlankFloor, TerrainDefOf.TileSandstone);
	}

	public static bool TryRandomInexpensiveFloor(out TerrainDef floor, Predicate<TerrainDef> validator = null)
	{
		Func<TerrainDef, float> costCalculator = delegate(TerrainDef x)
		{
			List<ThingDefCountClass> list = x.CostListAdjusted(null);
			float num2 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				num2 += (float)list[i].count * list[i].thingDef.BaseMarketValue;
			}
			return num2;
		};
		IEnumerable<TerrainDef> enumerable = DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef x) => x.BuildableByPlayer && x.terrainAffordanceNeeded != TerrainAffordanceDefOf.Bridgeable && !x.IsCarpet && BuildCostsAllowedInStuffGen(x) && (validator == null || validator(x)) && costCalculator(x) > 0f);
		float cheapest = -1f;
		foreach (TerrainDef item in enumerable)
		{
			float num = costCalculator(item);
			if (cheapest == -1f || num < cheapest)
			{
				cheapest = num;
			}
		}
		return enumerable.Where((TerrainDef x) => costCalculator(x) <= cheapest * 4f).TryRandomElement(out floor);
	}

	public static TerrainDef CorrespondingTerrainDef(ThingDef stuffDef, bool beautiful, Faction faction = null)
	{
		tmpFactionFloors.Clear();
		if (faction != null && faction.ideos != null)
		{
			foreach (TerrainDef item in IdeoFloorTypes(faction))
			{
				if (item.CostList == null)
				{
					continue;
				}
				for (int i = 0; i < item.CostList.Count; i++)
				{
					if (item.CostList[i].thingDef == stuffDef)
					{
						tmpFactionFloors.Add(item);
						break;
					}
				}
			}
			if (tmpFactionFloors.Any() && tmpFactionFloors.TryRandomElementByWeight(delegate(TerrainDef x)
			{
				float statOffsetFromList = x.statBases.GetStatOffsetFromList(StatDefOf.Beauty);
				if (statOffsetFromList == 0f)
				{
					return 0f;
				}
				return (!beautiful) ? (1f / statOffsetFromList) : statOffsetFromList;
			}, out var result))
			{
				return result;
			}
		}
		TerrainDef terrainDef = null;
		List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
		for (int num = 0; num < allDefsListForReading.Count; num++)
		{
			if (allDefsListForReading[num].CostList == null)
			{
				continue;
			}
			for (int num2 = 0; num2 < allDefsListForReading[num].CostList.Count; num2++)
			{
				if (allDefsListForReading[num].CostList[num2].thingDef == stuffDef && (terrainDef == null || (beautiful ? (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) < allDefsListForReading[num].statBases.GetStatOffsetFromList(StatDefOf.Beauty)) : (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) > allDefsListForReading[num].statBases.GetStatOffsetFromList(StatDefOf.Beauty)))))
				{
					terrainDef = allDefsListForReading[num];
				}
			}
		}
		if (terrainDef == null)
		{
			terrainDef = TerrainDefOf.Concrete;
		}
		return terrainDef;
	}

	public static TerrainDef RegionalRockTerrainDef(PlanetTile tile, bool beautiful)
	{
		ThingDef thingDef = Find.World.NaturalRockTypesIn(tile).RandomElementWithFallback()?.building.mineableThing;
		return CorrespondingTerrainDef((thingDef != null && thingDef.butcherProducts != null && thingDef.butcherProducts.Count > 0) ? thingDef.butcherProducts[0].thingDef : null, beautiful);
	}

	public static bool AnyDoorAdjacentCardinalTo(IntVec3 cell, Map map)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = cell + GenAdj.CardinalDirections[i];
			if (c.InBounds(map) && c.GetDoor(map) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyDoorAdjacentCardinalTo(CellRect rect, Map map)
	{
		foreach (IntVec3 item in rect.AdjacentCellsCardinal)
		{
			if (item.InBounds(map) && item.GetDoor(map) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static ThingDef WallStuffAt(IntVec3 c, Map map)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice != null && edifice.def == ThingDefOf.Wall)
		{
			return edifice.Stuff;
		}
		return null;
	}

	public static void ScatterSentryDronesInMap(SimpleCurve sentryCountFromPointsCurve, Map map, Faction faction, GenStepParams parms)
	{
		CellRect rect = MapGenerator.GetVar<CellRect>("SpawnRect");
		if (rect.Width <= 1 || rect.Height <= 1)
		{
			Log.Warning($"BaseGenUtility.ScatterSentryDronesInMap called but spawn rect wasn't valid: {rect}");
			return;
		}
		int num = Mathf.RoundToInt(sentryCountFromPointsCurve.Evaluate(parms.sitePart?.parms?.points ?? StorytellerUtility.DefaultThreatPointsNow(map)));
		List<Room> collection = map.regionGrid.AllRooms.Where((Room r) => r.ExtentsClose.FullyContainedWithin(rect) && !r.ExposedToSpace && !r.OutdoorsForWork && r.CellCount > 10).ToList();
		HashSet<Room> hashSet = new HashSet<Room>(collection);
		if (hashSet.Count == 0)
		{
			Log.Warning($"BaseGenUtility.ScatterSentryDronesInMap called but no valid rooms found in rect {rect} on map {map}.");
			return;
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			if (hashSet.Count == 0)
			{
				hashSet = new HashSet<Room>(collection);
			}
			Room room = hashSet.RandomElement();
			hashSet.Remove(room);
			IntVec3 intVec = room.Cells.Where((IntVec3 c) => c.Standable(map)).RandomElement();
			if (intVec == IntVec3.Invalid)
			{
				num2--;
			}
			else
			{
				GenSpawn.Spawn(PawnGenerator.GeneratePawn(PawnKindDefOf.Drone_Sentry, faction), intVec, map);
			}
		}
	}

	public static void CheckSpawnBridgeUnder(ThingDef thingDef, IntVec3 c, Rot4 rot)
	{
		if (thingDef.category != ThingCategory.Building)
		{
			return;
		}
		Map map = BaseGen.globalSettings.map;
		CellRect cellRect = GenAdj.OccupiedRect(c, rot, thingDef.size);
		bridgeCells.Clear();
		foreach (IntVec3 item in cellRect)
		{
			if (!item.SupportsStructureType(map, thingDef.terrainAffordanceNeeded) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, item, map, Rot4.North))
			{
				bridgeCells.Add(item);
			}
		}
		if (!bridgeCells.Any())
		{
			return;
		}
		if (thingDef.size.x != 1 || thingDef.size.z != 1)
		{
			for (int num = bridgeCells.Count - 1; num >= 0; num--)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = bridgeCells[num] + GenAdj.AdjacentCells[i];
					if (!bridgeCells.Contains(intVec) && intVec.InBounds(map) && !intVec.SupportsStructureType(map, thingDef.terrainAffordanceNeeded) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, intVec, map, Rot4.North))
					{
						bridgeCells.Add(intVec);
					}
				}
			}
		}
		for (int j = 0; j < bridgeCells.Count; j++)
		{
			map.terrainGrid.SetTerrain(bridgeCells[j], TerrainDefOf.Bridge);
		}
	}

	[DebugOutput]
	private static void WallStuffs()
	{
		DebugTables.MakeTablesDialog(GenStuff.AllowedStuffsFor(ThingDefOf.Wall), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("cheap", (ThingDef d) => (d.BaseMarketValue / d.VolumePerUnit < 5f).ToStringCheckBlank()), new TableDataGetter<ThingDef>("floor", (ThingDef d) => CorrespondingTerrainDef(d, beautiful: false).defName), new TableDataGetter<ThingDef>("floor (beautiful)", (ThingDef d) => CorrespondingTerrainDef(d, beautiful: true).defName));
	}

	public static void DoPathwayBetween(IntVec3 a, IntVec3 b, TerrainDef terrainDef, int size = 3)
	{
		foreach (IntVec3 item in GenSight.PointsOnLineOfSight(a, b))
		{
			foreach (IntVec3 item2 in CellRect.CenteredOn(item, size, size))
			{
				if (item2.InBounds(BaseGen.globalSettings.map))
				{
					BaseGen.globalSettings.map.terrainGrid.SetTerrain(item2, terrainDef);
				}
			}
		}
	}
}
