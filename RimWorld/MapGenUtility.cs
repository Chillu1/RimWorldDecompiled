using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LudeonTK;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

[BurstCompile]
public static class MapGenUtility
{
	public enum SearchWeightMode
	{
		None,
		Largest,
		Center
	}

	public struct CellData
	{
		public int x;

		public int z;
	}

	public class PostProcessSettlementParams
	{
		public bool clearBuildingFaction;

		public Faction faction;

		public bool noFuel;

		public bool ageCorpses;

		public bool damageBuildings;

		public bool canDamageWalls;
	}

	public delegate void ComputeLargestRects_0000B717_0024PostfixBurstDelegate(ref UnsafeList<CellRect> rects, ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, int minWidth, int minHeight, int maxWidth = -1, int maxHeight = -1, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8);

	internal static class ComputeLargestRects_0000B717_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(ComputeLargestRects_0000B717_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static ComputeLargestRects_0000B717_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref UnsafeList<CellRect> rects, ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, int minWidth, int minHeight, int maxWidth = -1, int maxHeight = -1, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref UnsafeList<CellRect>, ref NativeArray<CellData>, ref NativeArray<float>, ref CellIndices, int, int, int, int, float, float, int, void>)functionPointer)(ref rects, ref set, ref elevation, ref indices, minWidth, minHeight, maxWidth, maxHeight, minElevation, maxElevation, mapBorderPadding);
					return;
				}
			}
			ComputeLargestRects_0024BurstManaged(ref rects, ref set, in elevation, in indices, minWidth, minHeight, maxWidth, maxHeight, minElevation, maxElevation, mapBorderPadding);
		}
	}

	public delegate void RectsComputeSpaces_0000B718_0024PostfixBurstDelegate(ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, float minElevation, float maxElevation, int borderPadding);

	internal static class RectsComputeSpaces_0000B718_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(RectsComputeSpaces_0000B718_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static RectsComputeSpaces_0000B718_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, float minElevation, float maxElevation, int borderPadding)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<CellData>, ref NativeArray<float>, ref CellIndices, float, float, int, void>)functionPointer)(ref set, ref elevation, ref indices, minElevation, maxElevation, borderPadding);
					return;
				}
			}
			RectsComputeSpaces_0024BurstManaged(ref set, in elevation, in indices, minElevation, maxElevation, borderPadding);
		}
	}

	private static bool debug_WarnedMissingTerrain;

	private const float MaxWaterOverlap = 0.25f;

	private static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);

	private const int RequiredCellCount = 100;

	private static readonly IntRange DefaultLootCountRange = new IntRange(3, 10);

	private static readonly FloatRange DefaultLootMarketValue = new FloatRange(1800f, 2000f);

	private static readonly HashSet<int> tmpSetRoomIds = new HashSet<int>(8);

	public static bool ShouldGenerateBeachSand(IntVec3 cell, Map map)
	{
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
		if (!terrainDef.IsWater)
		{
			return !terrainDef.IsIce;
		}
		return false;
	}

	public static TerrainDef DeepFreshWaterTerrainAt(IntVec3 cell, Map map)
	{
		return map.BiomeAt(cell).waterDeepTerrain ?? TerrainDefOf.WaterDeep;
	}

	public static TerrainDef ShallowFreshWaterTerrainAt(IntVec3 cell, Map map)
	{
		return map.BiomeAt(cell).waterShallowTerrain ?? TerrainDefOf.WaterShallow;
	}

	public static TerrainDef DeepMovingWaterTerrainAt(IntVec3 cell, Map map)
	{
		return map.BiomeAt(cell).waterMovingChestDeepTerrain ?? TerrainDefOf.WaterMovingChestDeep;
	}

	public static TerrainDef ShallowMovingWaterTerrainAt(IntVec3 cell, Map map)
	{
		return map.BiomeAt(cell).waterMovingShallowTerrain ?? TerrainDefOf.WaterMovingShallow;
	}

	public static TerrainDef DeepOceanWaterTerrainAt(IntVec3 cell, Map map)
	{
		return map.BiomeAt(cell).oceanDeepTerrain ?? TerrainDefOf.WaterOceanDeep;
	}

	public static TerrainDef ShallowOceanWaterTerrainAt(IntVec3 cell, Map map)
	{
		return map.BiomeAt(cell).oceanShallowTerrain ?? TerrainDefOf.WaterOceanShallow;
	}

	public static TerrainDef BeachTerrainAt(IntVec3 cell, Map map)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			if (mutator.overrideCoastalBeachTerrain != null)
			{
				return mutator.overrideCoastalBeachTerrain;
			}
		}
		return map.BiomeAt(cell).coastalBeachTerrain ?? TerrainDefOf.Sand;
	}

	public static TerrainDef LakeshoreTerrainAt(IntVec3 cell, Map map)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			if (mutator.overrideLakeBeachTerrain != null)
			{
				return mutator.overrideLakeBeachTerrain;
			}
		}
		return map.BiomeAt(cell).lakeBeachTerrain ?? TerrainDefOf.Sand;
	}

	public static TerrainDef MudTerrainAt(IntVec3 cell, Map map)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			if (mutator.overrideMudTerrain != null)
			{
				return mutator.overrideMudTerrain;
			}
		}
		return map.BiomeAt(cell).mudTerrain ?? TerrainDefOf.Mud;
	}

	public static TerrainDef RiverbankTerrainAt(IntVec3 cell, Map map)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			if (mutator.overrideRiverbankTerrain != null)
			{
				return mutator.overrideRiverbankTerrain;
			}
		}
		return map.BiomeAt(cell).riverbankTerrain ?? TerrainDefOf.Riverbank;
	}

	public static void ScatterCorpses(CellRect rect, Map map, Faction faction, IntRange count, FloatRange deadDaysRange)
	{
		int randomInRange = count.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			IntVec3 cell = CellFinder.RandomClosewalkCellNear(rect.RandomCell, map, 10, Validator);
			if (cell.IsValid)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(faction.RandomPawnKind(), null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: true));
				KillSpawnCorpse(map, deadDaysRange, pawn, cell);
			}
		}
		bool Validator(IntVec3 pos)
		{
			if (pos.Standable(map))
			{
				return pos.GetFirstThing<Corpse>(map) == null;
			}
			return false;
		}
	}

	public static void ScatterCorpses(Map map, Faction faction, float points, FloatRange deadDaysRange)
	{
		foreach (Pawn item in PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			faction = faction,
			groupKind = PawnGroupKindDefOf.Settlement,
			points = points,
			tile = map.Tile
		}, warnOnZeroResults: false))
		{
			IntVec3 cell = CellFinder.RandomClosewalkCellNear(CellFinderLoose.TryFindCentralCell(map, 10, 5, (IntVec3 x) => x.Walkable(map)), map, 10);
			if (cell.IsValid)
			{
				KillSpawnCorpse(map, deadDaysRange, item, cell);
			}
			else
			{
				item.Discard();
			}
		}
	}

	private static void KillSpawnCorpse(Map map, FloatRange deadDaysRange, Pawn pawn, IntVec3 cell)
	{
		HealthUtility.SimulateKilled(pawn, DamageDefOf.Bullet);
		pawn.Corpse.Age = Mathf.RoundToInt(deadDaysRange.RandomInRange * 60000f - (float)Rand.Range(0, 60000));
		pawn.relations.hidePawnRelations = true;
		pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
		if (pawn.RaceProps.BloodDef != null)
		{
			GenSpawn.SpawnIrregularLump(pawn.RaceProps.BloodDef, cell, map, new IntRange(1, 5), new IntRange(2, 4));
		}
		GenSpawn.Spawn(pawn.Corpse, cell, map);
	}

	public static void DestroyTurrets(Map map)
	{
		foreach (Building item in map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.Turret_MiniTurret).ToList())
		{
			item.Kill();
		}
	}

	public static void DestroyProcessedFood(Map map)
	{
		foreach (Thing item in map.listerThings.AllThings.Where((Thing t) => t.def.IsIngestible && t.def.IsHumanFood()).ToList())
		{
			item.Destroy();
		}
	}

	public static void ForbidAllItems(Map map)
	{
		foreach (Thing allThing in map.listerThings.AllThings)
		{
			if (allThing.def.category == ThingCategory.Item)
			{
				CompForbiddable compForbiddable = allThing.TryGetComp<CompForbiddable>();
				if (compForbiddable != null && !compForbiddable.Forbidden)
				{
					allThing.SetForbidden(value: true, warnOnFail: false);
				}
			}
		}
	}

	public static void DamageBuildings(List<Building> buildings, Map map, float maxPercentDestroyed = 0.65f, float hpRandomFactor = 1.2f, float destroyChanceExponent = 1.32f)
	{
		if (buildings.NullOrEmpty())
		{
			return;
		}
		CellRect rect = buildings[0].OccupiedRect();
		for (int i = 1; i < buildings.Count; i++)
		{
			rect = rect.Encapsulate(buildings[i].OccupiedRect());
		}
		Rot4 random = Rot4.Random;
		int num = 0;
		int count = buildings.Count;
		foreach (Building building in buildings)
		{
			if (building.Destroyed)
			{
				continue;
			}
			DamageBuilding(building, map, rect, random, out var destroyed, hpRandomFactor, destroyChanceExponent);
			if (destroyed)
			{
				num++;
				ClearDisconnectedDoors(building, map);
				if ((float)num > (float)count * maxPercentDestroyed)
				{
					break;
				}
			}
		}
	}

	private static void ClearDisconnectedDoors(Building building, Map map)
	{
		foreach (IntVec3 item in building.OccupiedRect().AdjacentCellsCardinal)
		{
			Building_Door door = item.GetDoor(map);
			if (door == null || door.Destroyed)
			{
				continue;
			}
			bool flag = false;
			foreach (IntVec3 item2 in door.OccupiedRect().AdjacentCellsCardinal)
			{
				if (item2.GetEdifice(map) != null && item2.GetEdifice(map).def == ThingDefOf.Wall)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				door.Destroy();
			}
		}
	}

	private static void DamageBuilding(Building building, Map map, CellRect rect, Rot4 dir, out bool destroyed, float hpRandomFactor = 1.2f, float destroyChanceExponent = 1.32f)
	{
		float num = ((!dir.IsHorizontal) ? ((float)(building.Position.z - rect.minZ) / (float)rect.Height) : ((float)(building.Position.x - rect.minX) / (float)rect.Width));
		if (dir == Rot4.East || dir == Rot4.South)
		{
			num = 1f - num;
		}
		if (Rand.Chance(Mathf.Pow(num, destroyChanceExponent)))
		{
			if (building is IThingHolder thingHolder)
			{
				foreach (Thing item in thingHolder.GetDirectlyHeldThings().ToList())
				{
					item.Destroy();
				}
			}
			building.Destroy();
			destroyed = true;
			{
				foreach (IntVec3 item2 in building.OccupiedRect())
				{
					TerrainDef terrain = item2.GetTerrain(map);
					if (terrain.burnedDef != null)
					{
						map.terrainGrid.SetTerrain(item2, terrain.burnedDef);
					}
				}
				return;
			}
		}
		destroyed = false;
		building.HitPoints = Mathf.Clamp(Mathf.RoundToInt((float)building.MaxHitPoints * (1f - num) * Rand.Range(1f, hpRandomFactor)), 1, building.MaxHitPoints);
	}

	public static bool TryGetRandomClearRect(int width, int height, out CellRect rect, int maxWidth = -1, int maxHeight = -1, Predicate<CellRect> rectValidator = null, float minElevation = -1f, float maxElevation = 0.7f, SearchWeightMode weightMode = SearchWeightMode.Largest, int mapBorderPadding = 1)
	{
		List<CellRect> list = GetClearRects(width, height, maxWidth, maxHeight, minElevation, maxElevation, mapBorderPadding);
		rect = CellRect.Empty;
		if (!list.Any())
		{
			return false;
		}
		list.Shuffle();
		switch (weightMode)
		{
		case SearchWeightMode.Largest:
			list = list.OrderBy((CellRect r) => r.Area).ToList();
			break;
		case SearchWeightMode.Center:
			list = list.OrderBy((CellRect r) => r.CenterCell.DistanceToSquared(MapGenerator.mapBeingGenerated.Center)).ToList();
			break;
		}
		foreach (CellRect item in list)
		{
			for (int num = 0; num < 10; num++)
			{
				if (!item.TryFindRandomInnerRect(new IntVec2(width, height), out var rect2))
				{
					return false;
				}
				if (rectValidator == null || rectValidator(rect2))
				{
					rect = rect2;
					return true;
				}
			}
		}
		foreach (CellRect item2 in list)
		{
			for (int num2 = 0; num2 < item2.Width - width; num2++)
			{
				for (int num3 = 0; num3 < item2.Height - height; num3++)
				{
					CellRect cellRect = new CellRect(item2.minX + num2, item2.minZ + num3, width, height);
					if (rectValidator == null || rectValidator(cellRect))
					{
						rect = cellRect;
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool TryGetLargestClearRect(out CellRect rect, Func<CellRect, bool> rectValidator = null, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8)
	{
		List<CellRect> clearRects = GetClearRects(1, 1, -1, -1, minElevation, maxElevation, mapBorderPadding);
		if (rectValidator != null)
		{
			for (int num = clearRects.Count - 1; num >= 0; num--)
			{
				if (!rectValidator(clearRects[num]))
				{
					clearRects.RemoveAt(num);
				}
			}
		}
		if (clearRects.Any())
		{
			rect = clearRects.MaxBy((CellRect r) => r.Area);
			return true;
		}
		rect = CellRect.Empty;
		return false;
	}

	public static bool TryGetClosestClearRectTo(out CellRect rect, IntVec2 size, IntVec3 position, Func<CellRect, bool> rectValidator = null, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8)
	{
		List<CellRect> clearRects = GetClearRects(size.x, size.z, -1, -1, minElevation, maxElevation, mapBorderPadding);
		if (rectValidator != null)
		{
			for (int num = clearRects.Count - 1; num >= 0; num--)
			{
				if (!rectValidator(clearRects[num]))
				{
					clearRects.RemoveAt(num);
				}
			}
		}
		if (clearRects.Empty())
		{
			rect = CellRect.Empty;
			return false;
		}
		if (!clearRects.MinBy((CellRect r) => r.CenterCell.DistanceToSquared(position)).TryFindNearestInnerRectTo(size, position, out var rect2))
		{
			rect = CellRect.Empty;
			return false;
		}
		rect = rect2;
		return true;
	}

	public static List<CellRect> GetClearRects(int minWidth, int minHeight, int maxWidth = -1, int maxHeight = -1, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8)
	{
		if (!MapGenerator.TryGetVar<object>("Elevation", out var _))
		{
			Log.Error("Elevation grid did not exist - a map is not being generated. This can only be used as long as map generator data exists.");
			return new List<CellRect>();
		}
		CellIndices indices = MapGenerator.mapBeingGenerated.cellIndices;
		UnsafeList<CellRect> rects = new UnsafeList<CellRect>(12, Allocator.Persistent);
		NativeArray<CellData> set = new NativeArray<CellData>(indices.NumGridCells, Allocator.Persistent);
		ComputeLargestRects(ref rects, ref set, in MapGenerator.Elevation.Grid_Unsafe, in indices, minWidth, minHeight, maxWidth, maxHeight, minElevation, maxElevation, mapBorderPadding);
		List<CellRect> list = new List<CellRect>();
		NativeArrayUtility.CopyUnsafeListToList(list, rects);
		rects.Dispose();
		set.Dispose();
		return list;
	}

	[BurstCompile]
	private static void ComputeLargestRects(ref UnsafeList<CellRect> rects, ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, int minWidth, int minHeight, int maxWidth = -1, int maxHeight = -1, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8)
	{
		ComputeLargestRects_0000B717_0024BurstDirectCall.Invoke(ref rects, ref set, in elevation, in indices, minWidth, minHeight, maxWidth, maxHeight, minElevation, maxElevation, mapBorderPadding);
	}

	[BurstCompile]
	private static void RectsComputeSpaces(ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, float minElevation, float maxElevation, int borderPadding)
	{
		RectsComputeSpaces_0000B718_0024BurstDirectCall.Invoke(ref set, in elevation, in indices, minElevation, maxElevation, borderPadding);
	}

	public static void PostProcessSettlement(Map map, List<Building> placed, PostProcessSettlementParams parms)
	{
		foreach (Building item in placed)
		{
			if (parms.clearBuildingFaction)
			{
				if (item is Building_Turret)
				{
					continue;
				}
				if (item.Faction == parms.faction)
				{
					item.SetFaction(null);
				}
			}
			if (parms.noFuel && item.TryGetComp<CompRefuelable>(out var comp))
			{
				comp.ConsumeFuel(comp.Fuel);
			}
			if (parms.ageCorpses && item is Building_CorpseCasket { HasCorpse: not false } building_CorpseCasket)
			{
				int num = Rand.Range(0, 900000);
				building_CorpseCasket.Corpse.Age += num;
				building_CorpseCasket.Corpse.GetComp<CompRottable>().RotProgress += num;
			}
		}
		if (!parms.damageBuildings)
		{
			return;
		}
		if (!parms.canDamageWalls)
		{
			placed.RemoveWhere((Building building) => building.def == ThingDefOf.Wall);
		}
		DamageBuildings(placed, map, 0.6f, 1.2f, 1.2f);
	}

	public static bool IsMixedBiome(Map map)
	{
		return map.MixedBiomeComp.IsMixedBiome;
	}

	public static BiomeDef BiomeAt(this Map map, IntVec3 c)
	{
		return map.MixedBiomeComp.GetBiomeAt(c);
	}

	public static TerrainDef GetNaturalTerrainAt(IntVec3 cell, Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		MapGenFloatGrid fertility = MapGenerator.Fertility;
		MapGenFloatGrid caves = MapGenerator.Caves;
		Building edifice = cell.GetEdifice(map);
		if ((edifice != null && edifice.def.Fillage == FillCategory.Full) || caves[cell] > 0f)
		{
			return TerrainFrom(cell, map, elevation[cell], fertility[cell], preferRock: true);
		}
		return TerrainFrom(cell, map, elevation[cell], fertility[cell], preferRock: false);
	}

	public static TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, bool preferRock)
	{
		TerrainDef terrainDef = null;
		BiomeDef biomeDef = map.BiomeAt(c);
		bool flag = map.TileInfo.Mutators.Any((TileMutatorDef m) => m.preventsPondGeneration);
		if (!map.TileInfo.Mutators.Any((TileMutatorDef m) => m.preventPatches))
		{
			foreach (TerrainPatchMaker terrainPatchMaker in biomeDef.terrainPatchMakers)
			{
				if (!flag || !terrainPatchMaker.isPond)
				{
					terrainDef = terrainPatchMaker.TerrainAt(c, map, fertility);
					if (terrainDef != null)
					{
						break;
					}
				}
			}
		}
		if (terrainDef == null)
		{
			if (elevation > 0.55f && elevation < 0.61f && !biomeDef.noGravel)
			{
				terrainDef = biomeDef.gravelTerrain ?? TerrainDefOf.Gravel;
			}
			else if (elevation >= 0.61f)
			{
				terrainDef = GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
			}
		}
		if (terrainDef == null)
		{
			terrainDef = TerrainThreshold.TerrainAtValue(biomeDef.terrainsByFertility, fertility);
		}
		if (terrainDef == null)
		{
			if (!debug_WarnedMissingTerrain)
			{
				Log.Error("No terrain found in biome " + biomeDef.defName + " for elevation=" + elevation + ", fertility=" + fertility);
				debug_WarnedMissingTerrain = true;
			}
			terrainDef = TerrainDefOf.Sand;
		}
		if (preferRock && terrainDef.supportsRock)
		{
			terrainDef = GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
		}
		return terrainDef;
	}

	public static List<CellRect> GetLargestClearRects(Map map, IntVec2 minSize, IntVec2 maxSize, bool checkUsedRects, float minElevation = -1f, float maxElevation = 0.7f, TerrainAffordanceDef affordance = null, bool checkWaterRoads = true)
	{
		List<CellRect> clearRects = GetClearRects(minSize.x, minSize.z, maxSize.x, maxSize.z, minElevation, maxElevation);
		for (int i = 0; i < clearRects.Count; i++)
		{
			CellRect cellRect = clearRects[i];
			if (cellRect.Width > maxSize.x)
			{
				cellRect.Width = maxSize.x;
			}
			if (cellRect.Height > maxSize.z)
			{
				cellRect.Height = maxSize.z;
			}
			if (clearRects[i].TryFindRandomInnerRect(new IntVec2(cellRect.Width, cellRect.Height), out var rect))
			{
				clearRects[i] = rect;
			}
		}
		TryFixInvalidRects(clearRects, map, minSize.x, checkUsedRects, affordance, checkWaterRoads);
		RemoveInvalidRects(clearRects, map, checkUsedRects, affordance, checkWaterRoads);
		return clearRects;
	}

	public static void TryFixInvalidRects(List<CellRect> rects, Map map, int minRegionSize, bool checkUsedRects, TerrainAffordanceDef affordance, bool checkWaterRoads)
	{
		for (int num = rects.Count - 1; num >= 0; num--)
		{
			CellRect cellRect = rects[num];
			bool flag = true;
			int num2 = 100;
			while (flag && num2-- > 0)
			{
				flag = false;
				if (cellRect.Width < minRegionSize || cellRect.Height < minRegionSize)
				{
					rects.RemoveAt(num);
					break;
				}
				foreach (IntVec3 cell in cellRect.Cells)
				{
					if (!IsCellValid(cell, map, checkUsedRects, affordance, checkWaterRoads))
					{
						if (!cellRect.TryContractToRemove(cell, out var rect) || rect.Width < minRegionSize || rect.Height < minRegionSize)
						{
							rects.RemoveAt(num);
							break;
						}
						CellRect cellRect2 = (rects[num] = rect);
						cellRect = cellRect2;
						flag = true;
						break;
					}
				}
			}
			if (num2 <= 0)
			{
				Log.ErrorOnce("Loop guard exceeded trying to remove points from chunk rects", 956842091);
			}
		}
	}

	public static void RemoveInvalidRects(List<CellRect> rects, Map map, bool checkUsedRects, TerrainAffordanceDef affordance, bool checkWaterRoads)
	{
		for (int num = rects.Count - 1; num >= 0; num--)
		{
			foreach (IntVec3 cell in rects[num].Cells)
			{
				if (!IsCellValid(cell, map, checkUsedRects, affordance, checkWaterRoads))
				{
					rects.RemoveAt(num);
					break;
				}
			}
		}
	}

	public static bool IsCellValid(IntVec3 cell, Map map, bool checkUsedRects, TerrainAffordanceDef affordance, bool checkWaterRoads)
	{
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
		if (checkWaterRoads && (terrainDef.IsWater || terrainDef.IsRoad))
		{
			return false;
		}
		if (affordance != null && !cell.SupportsStructureType(map, affordance))
		{
			return false;
		}
		if (checkUsedRects && MapGenerator.UsedRects.Any((CellRect rect) => rect.Contains(cell)))
		{
			return false;
		}
		List<Thing> thingList = cell.GetThingList(map);
		for (int num = 0; num < thingList.Count; num++)
		{
			if (!thingList[num].def.destroyable)
			{
				return false;
			}
			if (thingList[num] is Building building && building.def.IsBuildingArtificial && !building.def.IsNonDeconstructibleAttackableBuilding && !building.IsClearableFreeBuilding)
			{
				return false;
			}
		}
		return true;
	}

	public static bool TryGetStructureRect(Map map, IntRange structureSizeRange, out CellRect rect, Predicate<CellRect> extraValidator = null)
	{
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		int structureWidth = structureSizeRange.RandomInRange;
		int structureHeight = structureSizeRange.RandomInRange;
		int structureArea = structureWidth * structureHeight;
		int min = structureSizeRange.min;
		if (!TryGetClosestClearRectTo(out rect, new IntVec2(structureWidth, structureHeight), map.Center, Validator) && !TryGetClosestClearRectTo(out rect, new IntVec2(min, min), map.Center, Validator) && !TryGetRandomClearRect(min, min, out rect, -1, -1, Validator))
		{
			if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c.RectAbout(structureWidth, structureHeight)), out var result))
			{
				Log.Error("Failed to find location structure");
				return false;
			}
			rect = result.RectAbout(structureWidth, structureHeight);
		}
		return true;
		bool Validator(CellRect r)
		{
			if (!r.FullyContainedWithin(map.BoundsRect().ContractedBy(10)))
			{
				return false;
			}
			if ((float)r.Cells.Count((IntVec3 c) => c.GetTerrain(map).IsWater) > (float)structureArea * 0.25f)
			{
				return false;
			}
			if (usedRects.Any((CellRect ur) => ur.Overlaps(r)))
			{
				return false;
			}
			if (!r.CenterCell.InHorDistOf(map.Center, (float)map.Size.x * 0.75f))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(r))
			{
				return false;
			}
			return true;
		}
	}

	public static void GeneratePawns(Map map, CellRect rect, Faction faction, Lord lord, PawnGroupKindDef pawnGroupKindDef = null, PawnGroupMakerParms pawnGroupMakerParams = null, float? points = null, int? seed = null, Predicate<IntVec3> cellValidator = null, bool requiresRoof = false)
	{
		if (pawnGroupMakerParams == null)
		{
			pawnGroupMakerParams = new PawnGroupMakerParms
			{
				tile = map.Tile,
				faction = faction,
				points = (points ?? DefaultPawnsPoints.RandomInRange),
				inhabitants = true,
				seed = seed
			};
		}
		pawnGroupMakerParams.groupKind = pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
		foreach (Pawn item in PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParams))
		{
			if (TryFindSpawnCell(item, out var cell))
			{
				GenSpawn.Spawn(item, cell, map);
				lord?.AddPawn(item);
			}
			else
			{
				Find.WorldPawns.PassToWorld(item);
			}
		}
		bool TryFindSpawnCell(Pawn pawn, out IntVec3 result)
		{
			return CellFinder.TryFindRandomCellInsideWith(rect, Validator, out result);
			bool Validator(IntVec3 x)
			{
				if (!x.Standable(map))
				{
					return false;
				}
				if (requiresRoof && x.GetRoof(map) == null)
				{
					return false;
				}
				if (x.GetEdifice(map) != null)
				{
					return false;
				}
				if (pawn.ConcernedByVacuum)
				{
					Room room = x.GetRoom(map);
					if (room == null || room.ExposedToSpace)
					{
						return false;
					}
				}
				if (!x.ConnectedCellCountExceeds(map, 100))
				{
					return false;
				}
				if (cellValidator != null && !cellValidator(x))
				{
					return false;
				}
				return true;
			}
		}
	}

	public static void GenerateLoot(Map map, CellRect rect, ThingSetMakerDef setMakerDef, FloatRange? marketValueRange = null, IntRange? countRange = null, Faction faction = null, bool requiresRoof = false)
	{
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			countRange = (countRange ?? DefaultLootCountRange),
			techLevel = (faction?.def.techLevel ?? TechLevel.Undefined),
			makingFaction = faction,
			totalMarketValueRange = (marketValueRange ?? DefaultLootMarketValue)
		};
		GenerateLoot(map, rect, setMakerDef.root.Generate(parms), requiresRoof);
	}

	public static void GenerateLoot(Map map, CellRect rect, IList<Thing> things, bool requiresRoof = false)
	{
		List<IntVec3> list = rect.Cells.Where(CanPlace).ToList();
		while (list.Count > 0 && things.Count > 0)
		{
			int index = Rand.Range(0, list.Count);
			IntVec3 cell = list[index];
			list.RemoveAt(index);
			index = Rand.Range(0, things.Count);
			Thing thing = things[index];
			things.RemoveAt(index);
			SpawnLoot(thing, cell, map);
		}
		if (things.Count <= 0)
		{
			return;
		}
		Log.Warning("Could not scatter loot things in rooms: " + string.Join(", ", things.Select((Thing t) => t.Label)));
		foreach (Thing thing2 in things)
		{
			for (int num = 1000; num > 0; num--)
			{
				IntVec3 cell2 = CellFinder.RandomCell(map);
				if (CanPlace(cell2))
				{
					SpawnLoot(thing2, cell2, map);
				}
			}
		}
		bool CanPlace(IntVec3 intVec)
		{
			if (intVec.GetFirstItem(map) != null)
			{
				return false;
			}
			if (!intVec.Standable(map))
			{
				return false;
			}
			if (requiresRoof && intVec.GetRoof(map) == null)
			{
				return false;
			}
			if (intVec.GetRoom(map).PsychologicallyOutdoors)
			{
				return false;
			}
			return true;
		}
	}

	private static void SpawnLoot(Thing thing, IntVec3 cell, Map map)
	{
		GenSpawn.Spawn(thing, cell, map);
		thing.TrySetForbidden(value: true);
	}

	public static bool IsGoodSpawnCell(ThingDef def, IntVec3 pos, Map map)
	{
		CellRect cellRect = pos.RectAbout(def.Size);
		if (!GenSpawn.CanSpawnAt(def, pos, map))
		{
			return false;
		}
		foreach (IntVec3 cell in cellRect.Cells)
		{
			if (cell.GetEdifice(map) != null)
			{
				return false;
			}
			foreach (Thing thing in cell.GetThingList(map))
			{
				if (thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Pawn)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void SetMapRoomTemperature(Map map, LayoutDef layoutDef, float temperature)
	{
		if (map.Biome.constantOutdoorTemperature.HasValue && Mathf.Approximately(map.Biome.constantOutdoorTemperature.Value, temperature))
		{
			return;
		}
		tmpSetRoomIds.Clear();
		foreach (LayoutStructureSketch layoutStructureSketch in map.layoutStructureSketches)
		{
			if (layoutStructureSketch.layoutDef != layoutDef)
			{
				continue;
			}
			foreach (LayoutRoom room2 in layoutStructureSketch.structureLayout.Rooms)
			{
				foreach (CellRect rect in room2.rects)
				{
					foreach (IntVec3 cell in rect.Cells)
					{
						if (cell.GetRoof(map) != null)
						{
							Room room = cell.GetRoom(map);
							if (room != null && tmpSetRoomIds.Add(room.ID))
							{
								room.Temperature = temperature;
							}
							if (map.Biome.inVacuum && room != null && !room.ExposedToSpace)
							{
								room.Vacuum = 0f;
							}
						}
					}
				}
			}
		}
	}

	public static void SpawnScatter(Map map, ThingDef thingDef, FloatRange countPer10K)
	{
		float num = (float)map.Size.x * (float)map.Size.z / 10000f;
		int num2 = Mathf.RoundToInt(countPer10K.RandomInRange * num);
		List<CellRect> list = new List<CellRect>();
		foreach (LayoutStructureSketch layoutStructureSketch in map.layoutStructureSketches)
		{
			foreach (LayoutRoom room in layoutStructureSketch.structureLayout.Rooms)
			{
				list.AddRange(room.rects);
			}
		}
		if (list.Empty())
		{
			return;
		}
		for (int i = 0; i < num2; i++)
		{
			CellRect cellRect = list.RandomElementByWeight((CellRect x) => x.Area);
			for (int num3 = 0; num3 < 10; num3++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				Thing thing;
				if (thingDef.IsFilth)
				{
					if (FilthMaker.TryMakeFilth(randomCell, map, thingDef))
					{
						break;
					}
				}
				else if (GenSpawn.TrySpawn(thingDef, randomCell, map, Rot4.Random, out thing))
				{
					break;
				}
			}
		}
	}

	public static List<CellRect> SubdivideRectIntoChunks(CellRect bounds, int minRegionSize, int separation)
	{
		int subdivisionCount = GetSubdivisionCount(bounds, ref minRegionSize);
		return bounds.Subdivide(subdivisionCount, separation);
	}

	public static int GetSubdivisionCount(CellRect bounds, ref int minRegionSize)
	{
		int num = 1;
		int num2;
		for (num2 = bounds.Size.x / 2; num2 > minRegionSize; num2 /= 2)
		{
			num++;
		}
		if (num2 < minRegionSize)
		{
			num--;
			num2 *= 2;
		}
		minRegionSize = num2;
		return num;
	}

	public static void SpawnExteriorLumps(Map map, ThingDef thingDef, FloatRange countPer10K, IntRange countRange, IntRange distRange, int maxDistFromBuilding = 6)
	{
		float num = (float)map.Size.x * (float)map.Size.z / 10000f;
		int num2 = Mathf.RoundToInt(countPer10K.RandomInRange * num);
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>(from c in map.layoutStructureSketches.SelectMany((LayoutStructureSketch sketch) => sketch.structureLayout.Rooms.SelectMany((LayoutRoom layoutRoom) => layoutRoom.rects.SelectMany((CellRect rect) => rect.DifferenceCells(rect.ExpandedBy(maxDistFromBuilding)))))
			where c.InBounds(map) && c.GetEdifice(map) == null
			select c);
		foreach (LayoutStructureSketch layoutStructureSketch in map.layoutStructureSketches)
		{
			foreach (LayoutRoom room in layoutStructureSketch.structureLayout.Rooms)
			{
				hashSet.RemoveWhere((IntVec3 c) => room.Contains(c, 1));
			}
		}
		if (!hashSet.NullOrEmpty())
		{
			for (int num3 = 0; num3 < num2; num3++)
			{
				IntVec3 pos = hashSet.RandomElement();
				GenSpawn.SpawnIrregularLump(thingDef, pos, map, countRange, distRange, WipeMode.Vanish, Validator);
			}
		}
		bool Validator(IntVec3 p)
		{
			if (p.GetEdifice(map) == null)
			{
				return p.Standable(map);
			}
			return false;
		}
	}

	public static int DoRectEdgeLumps(Map map, TerrainDef terrain, ref CellRect rect, IntRange lengthRange, IntRange offsetRange, TerrainDef scatter = null)
	{
		int num = 0;
		int lumpLength = lengthRange.RandomInRange;
		int lumpOffset = offsetRange.RandomInRange;
		for (int i = rect.minX; i < rect.maxX; i++)
		{
			num += DoHorizontalLumps(i, rect.maxZ, terrain, map, ref lumpOffset, ref lumpLength, positive: true, lengthRange, offsetRange, scatter);
		}
		for (int j = rect.minX; j < rect.maxX; j++)
		{
			num += DoHorizontalLumps(j, rect.minZ, terrain, map, ref lumpOffset, ref lumpLength, positive: false, lengthRange, offsetRange, scatter);
		}
		for (int k = rect.minZ; k < rect.maxZ; k++)
		{
			num += DoVerticalLumps(k, rect.maxX, terrain, map, ref lumpOffset, ref lumpLength, positive: true, lengthRange, offsetRange, scatter);
		}
		for (int l = rect.minZ; l < rect.maxZ; l++)
		{
			num += DoVerticalLumps(l, rect.minX, terrain, map, ref lumpOffset, ref lumpLength, positive: false, lengthRange, offsetRange, scatter);
		}
		rect = rect.ExpandedBy(offsetRange.max);
		return num;
	}

	public static int DoHorizontalLumps(int x, int worldZ, TerrainDef terrain, Map map, ref int lumpOffset, ref int lumpLength, bool positive, IntRange lengthRange, IntRange offsetRange, TerrainDef scatter = null)
	{
		int num = 0;
		for (int i = 1; i <= lumpOffset; i++)
		{
			IntVec3 c = new IntVec3(x, 0, worldZ + i * (positive ? 1 : (-1)));
			if (!c.InBounds(map))
			{
				continue;
			}
			TerrainDef terrainDef = terrain;
			if (scatter != null && Rand.Bool)
			{
				terrainDef = scatter;
			}
			if (terrainDef == null)
			{
				List<Thing> thingList = c.GetThingList(map);
				for (int num2 = thingList.Count - 1; num2 >= 0; num2--)
				{
					thingList[num2].Destroy();
				}
				map.roofGrid.SetRoof(c, null);
			}
			else
			{
				map.terrainGrid.SetTerrain(c, terrainDef);
			}
			num++;
		}
		if (lumpLength-- <= 0)
		{
			ResetLumpVariables(ref lumpOffset, ref lumpLength, lengthRange, offsetRange);
		}
		return num;
	}

	public static int DoVerticalLumps(int z, int worldX, TerrainDef terrain, Map map, ref int lumpOffset, ref int lumpLength, bool positive, IntRange lengthRange, IntRange offsetRange, TerrainDef scatter = null)
	{
		int num = 0;
		for (int i = 1; i <= lumpOffset; i++)
		{
			IntVec3 c = new IntVec3(worldX + i * (positive ? 1 : (-1)), 0, z);
			if (!c.InBounds(map))
			{
				continue;
			}
			TerrainDef terrainDef = terrain;
			if (scatter != null && Rand.Bool)
			{
				terrainDef = scatter;
			}
			if (terrainDef == null)
			{
				List<Thing> thingList = c.GetThingList(map);
				for (int num2 = thingList.Count - 1; num2 >= 0; num2--)
				{
					thingList[num2].Destroy();
				}
				map.roofGrid.SetRoof(c, null);
			}
			else
			{
				map.terrainGrid.SetTerrain(c, terrainDef);
			}
			num++;
		}
		if (lumpLength-- <= 0)
		{
			ResetLumpVariables(ref lumpOffset, ref lumpLength, lengthRange, offsetRange);
		}
		return num;
	}

	private static void ResetLumpVariables(ref int lumpOffset, ref int lumpLength, IntRange lengthRange, IntRange offsetRange)
	{
		int num = lumpLength;
		int num2 = lumpOffset;
		lumpLength = lengthRange.RandomInRange;
		lumpOffset = offsetRange.RandomInRange;
		if (num == lumpLength)
		{
			lumpLength = lengthRange.RandomInRange;
		}
		if (num2 == lumpOffset)
		{
			lumpOffset = offsetRange.RandomInRange;
		}
	}

	public static void SpawnScatteredGroupPrefabs(Map map, CellRect structure, List<ScatteredPrefabs> groups)
	{
		foreach (ScatteredPrefabs group in groups)
		{
			GenStep_ScatterGroupPrefabs genStep_ScatterGroupPrefabs = new GenStep_ScatterGroupPrefabs();
			GenStepParams parms = default(GenStepParams);
			genStep_ScatterGroupPrefabs.minSpacing = group.minSpacing;
			genStep_ScatterGroupPrefabs.countPer10kCellsRange = group.countPer10kCellsRange;
			genStep_ScatterGroupPrefabs.groups = group.prefabs;
			genStep_ScatterGroupPrefabs.validators.Add(new StructureScatterValidator
			{
				structureRect = structure,
				maxDistFromStructure = group.maxDistFromStructure
			});
			genStep_ScatterGroupPrefabs.Generate(map, parms);
		}
	}

	[Obsolete("Use Line_NewTemp")]
	public static void Linea(TerrainDef terrain, Map map, IntVec3 start, IntVec3 end, float thickness, bool canExit = false)
	{
	}

	public static void Line_NewTemp(TerrainDef terrain, Map map, IntVec3 start, IntVec3 end, float thickness, bool canExit = false, TerrainDef replace = null)
	{
		int num = Mathf.CeilToInt(thickness / 2f);
		int num2 = start.x;
		int num3 = start.z;
		int x = end.x;
		int z = end.z;
		int num4 = Mathf.Abs(x - num2);
		int num5 = Mathf.Abs(z - num3);
		int num6 = ((num2 < x) ? 1 : (-1));
		int num7 = ((num3 < z) ? 1 : (-1));
		int num8 = num4 - num5;
		while (true)
		{
			bool flag = false;
			for (int i = -num; i <= num; i++)
			{
				for (int j = -num; j <= num; j++)
				{
					int num9 = num2 + i;
					int num10 = num3 + j;
					if (Vector2.SqrMagnitude(new Vector2(num2, num3) - new Vector2(num9, num10)) <= (float)(num * num))
					{
						IntVec3 intVec = new IntVec3(num9, 0, num10);
						if (!AnyTerrainAtCellMatches(terrain, intVec, map) && (replace == null || AnyTerrainAtCellMatches(replace, intVec, map)))
						{
							map.terrainGrid.SetTerrain(intVec, terrain);
							flag = true;
						}
					}
				}
			}
			if ((!canExit || flag) && (num2 != x || num3 != z))
			{
				int num11 = 2 * num8;
				if (num11 > -num5)
				{
					num8 -= num5;
					num2 += num6;
				}
				if (num11 < num4)
				{
					num8 += num4;
					num3 += num7;
				}
				continue;
			}
			break;
		}
	}

	private static bool AnyTerrainAtCellMatches(TerrainDef terrain, IntVec3 cell, Map map)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		if (terrain.temporary && map.terrainGrid.TempTerrainAt(cell) == terrain)
		{
			return true;
		}
		if (terrain.isFoundation && map.terrainGrid.FoundationAt(cell) == terrain)
		{
			return true;
		}
		if (!terrain.isFoundation && map.terrainGrid.TerrainAt(cell) == terrain)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static void ComputeLargestRects_0024BurstManaged(ref UnsafeList<CellRect> rects, ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, int minWidth, int minHeight, int maxWidth = -1, int maxHeight = -1, float minElevation = -1f, float maxElevation = 0.7f, int mapBorderPadding = 8)
	{
		RectsComputeSpaces(ref set, in elevation, in indices, minElevation, maxElevation, mapBorderPadding);
		for (int num = indices.NumGridCells - 1; num >= 0; num--)
		{
			if (set[num].z >= minHeight)
			{
				IntVec3 c = indices.IndexToCell(num);
				bool flag = false;
				int num2 = set[num].z;
				int num3 = set[num].x;
				if (num3 >= minWidth && num2 >= minHeight)
				{
					for (int i = 0; i < rects.Length; i++)
					{
						if (rects[i].Contains(c))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						for (int num4 = c.z - 1; num4 >= c.z - num2; num4--)
						{
							IntVec3 c2 = new IntVec3(c.x, 0, num4);
							int index = indices.CellToIndex(c2);
							CellData cellData = set[index];
							if (cellData.x < minWidth)
							{
								num2 = c.z - num4;
								break;
							}
							num3 = math.min(num3, cellData.x);
						}
						if (num3 >= minWidth && num2 >= minHeight)
						{
							if (maxWidth > 0)
							{
								num3 = math.min(num3, maxWidth);
							}
							if (maxHeight > 0)
							{
								num2 = math.min(num2, maxHeight);
							}
							CellRect value = new CellRect(c.x - num3 + 1, c.z - num2 + 1, num3, num2);
							bool flag2 = false;
							for (int j = 0; j < rects.Length; j++)
							{
								if (rects[j].Overlaps(value))
								{
									if (value.Area <= rects[j].Area)
									{
										flag = true;
										break;
									}
									flag2 = true;
								}
							}
							if (!flag)
							{
								if (flag2)
								{
									for (int num5 = rects.Length - 1; num5 >= 0; num5--)
									{
										if (rects[num5].Overlaps(value))
										{
											rects.RemoveAt(num5);
										}
									}
								}
								rects.Add(in value);
							}
						}
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static void RectsComputeSpaces_0024BurstManaged(ref NativeArray<CellData> set, in NativeArray<float> elevation, in CellIndices indices, float minElevation, float maxElevation, int borderPadding)
	{
		for (int i = 0; i < indices.SizeX; i++)
		{
			if (set[i].z != 0)
			{
				continue;
			}
			IntVec3 intVec = indices.IndexToCell(i);
			int num = 1;
			for (int j = 0; j < indices.SizeZ; j++)
			{
				IntVec3 c = new IntVec3(intVec.x, 0, j);
				int index = indices.CellToIndex(c);
				float num2 = elevation[index];
				if (num2 <= minElevation || num2 >= maxElevation || c.x < borderPadding || c.z < borderPadding || c.x > indices.SizeX - borderPadding || c.z > indices.SizeZ - borderPadding)
				{
					set[index] = new CellData
					{
						x = -1,
						z = -1
					};
					num = 1;
					continue;
				}
				IntVec3 c2 = new IntVec3(intVec.x - 1, 0, j);
				int index2 = indices.CellToIndex(c2);
				int x = 1;
				if (indices.Contains(c2) && set[index2].x > 0)
				{
					x = set[index2].x + 1;
				}
				set[index] = new CellData
				{
					x = x,
					z = num++
				};
			}
		}
	}
}
