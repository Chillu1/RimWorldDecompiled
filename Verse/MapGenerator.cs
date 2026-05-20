using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class MapGenerator
{
	public static Map mapBeingGenerated;

	private static IntVec3 playerStartSpotInt = IntVec3.Invalid;

	private static Gravship gravship;

	public static List<IntVec3> rootsToUnfog = new List<IntVec3>();

	private static Dictionary<string, object> data = new Dictionary<string, object>();

	private static List<GenStepWithParams> tmpGenSteps = new List<GenStepWithParams>();

	private static int debugSeed;

	public static bool debugMode = false;

	private static int debugGenStepIndex = 0;

	private static List<CellRect> cachedUsedRects = new List<CellRect>();

	public const string ElevationName = "Elevation";

	public const string FertilityName = "Fertility";

	public const string CavesName = "Caves";

	public const string RectOfInterestName = "RectOfInterest";

	public const string UsedRectsName = "UsedRects";

	public const string RectOfInterestTurretsGenStepsCount = "RectOfInterestTurretsGenStepsCount";

	public const string DontGenerateClearedGravShipTerrain = "DontGenerateClearedGravShipTerrain";

	public const string GravshipSpawnSet = "GravshipSpawnSet";

	public const string SpawnRectName = "SpawnRect";

	public static List<MapGenUtility.CellData> tmpDataDump = new List<MapGenUtility.CellData>();

	public static MapGenFloatGrid Elevation => FloatGridNamed("Elevation");

	public static MapGenFloatGrid Fertility => FloatGridNamed("Fertility");

	public static MapGenFloatGrid Caves => FloatGridNamed("Caves");

	public static List<CellRect> UsedRects => GetOrGenerateVar<List<CellRect>>("UsedRects");

	public static IntVec3 PlayerStartSpot
	{
		get
		{
			if (!PlayerStartSpotValid)
			{
				Log.Error("Accessing player start spot before setting it.");
				return IntVec3.Invalid;
			}
			return playerStartSpotInt;
		}
		set
		{
			playerStartSpotInt = value;
		}
	}

	public static bool PlayerStartSpotValid => playerStartSpotInt.IsValid;

	public static Map GenerateMap(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null, bool isPocketMap = false, bool stepDebugger = false)
	{
		ProgramState programState = Current.ProgramState;
		Current.ProgramState = ProgramState.MapInitializing;
		ClearWorkingData();
		playerStartSpotInt = IntVec3.Invalid;
		rootsToUnfog.Clear();
		mapBeingGenerated = null;
		gravship = null;
		DeepProfiler.Start("InitNewGeneratedMap");
		Rand.PushState();
		int seed = Gen.HashCombineInt(Find.World.info.Seed, parent?.Tile.GetHashCode() ?? 0);
		if (isPocketMap)
		{
			seed = Gen.HashCombineInt(Find.World.info.Seed, parent?.ID ?? Rand.Int);
		}
		Rand.Seed = seed;
		if (stepDebugger)
		{
			debugMode = true;
			debugSeed = seed;
			debugGenStepIndex = 0;
		}
		try
		{
			if (parent != null && parent.HasMap)
			{
				Log.Error($"Tried to generate a new map and set {parent} as its parent, but this world object already has a map. One world object can't have more than 1 map.");
				parent = null;
			}
			DeepProfiler.Start("Set up map");
			Map map = new Map();
			map.uniqueID = Find.UniqueIDsManager.GetNextMapID();
			map.generationTick = GenTicks.TicksGame;
			map.events = new MapEvents(map);
			mapBeingGenerated = map;
			map.info.Size = mapSize;
			map.info.parent = parent;
			if (ModsConfig.OdysseyActive && extraGenStepDefs != null && extraGenStepDefs.Any((GenStepWithParams step) => step.def == GenStepDefOf.ReserveGravshipArea))
			{
				map.wasSpawnedViaGravShipLanding = true;
			}
			if (mapGenerator == null)
			{
				Log.Error("Attempted to generate map without generator; falling back on encounter map");
				mapGenerator = MapGeneratorDefOf.Encounter;
			}
			map.generatorDef = mapGenerator;
			map.info.disableSunShadows = mapGenerator.disableShadows;
			if (isPocketMap)
			{
				map.info.isPocketMap = true;
				map.pocketTileInfo = new Tile
				{
					PrimaryBiome = mapGenerator.pocketMapProperties.biome
				};
				foreach (TileMutatorDef tileMutator in mapGenerator.pocketMapProperties.tileMutators)
				{
					map.TileInfo.AddMutator(tileMutator);
				}
			}
			map.ConstructComponents();
			foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
			{
				mutator.Worker?.Init(map);
			}
			DeepProfiler.End();
			Current.Game.AddMap(map);
			if (mapGenerator.isUnderground)
			{
				foreach (IntVec3 allCell in map.AllCells)
				{
					map.roofGrid.SetRoof(allCell, mapGenerator.roofDef ?? RoofDefOf.RoofRockThick);
				}
			}
			extraInitBeforeContentGen?.Invoke(map);
			IEnumerable<GenStepWithParams> enumerable = mapGenerator.genSteps.Where(IsValidBiome).Select(GetGenStepParms);
			foreach (TileMutatorDef mutator2 in map.TileInfo.Mutators)
			{
				if (mutator2.extraGenSteps.Any())
				{
					enumerable = enumerable.Concat(mutator2.extraGenSteps.Select(GetGenStepParms));
				}
			}
			if (map.Biome.extraGenSteps.Any())
			{
				enumerable = enumerable.Concat(map.Biome.extraGenSteps.Where(IsValidBiome).Select(GetGenStepParms));
			}
			if (map.Biome.preventGenSteps.Any())
			{
				enumerable = enumerable.Where((GenStepWithParams step) => !map.Biome.preventGenSteps.Contains(step.def));
			}
			foreach (TileMutatorDef mut in map.TileInfo.Mutators)
			{
				if (mut.preventGenSteps.Any())
				{
					enumerable = enumerable.Where((GenStepWithParams step) => !mut.preventGenSteps.Contains(step.def));
				}
			}
			if (extraGenStepDefs != null)
			{
				enumerable = enumerable.Concat(extraGenStepDefs);
			}
			enumerable = enumerable.Distinct();
			map.areaManager.AddStartingAreas();
			map.weatherDecider.StartInitialWeather();
			DeepProfiler.Start("Generate contents into map");
			GenerateContentsIntoMap(enumerable, map, seed, stepDebugger);
			DeepProfiler.End();
			Find.Scenario.PostMapGenerate(map);
			DeepProfiler.Start("Finalize map init");
			map.FinalizeInit();
			DeepProfiler.End();
			DeepProfiler.Start("MapComponent.MapGenerated()");
			MapComponentUtility.MapGenerated(map);
			DeepProfiler.End();
			parent?.PostMapGenerate();
			DeepProfiler.Start("Map generator post init");
			if (!stepDebugger)
			{
				MapGeneratorPostInit(enumerable, map);
			}
			DeepProfiler.End();
			if (gravship != null && !stepDebugger && (bool)map.Parent.CanBeSettled)
			{
				GravshipUtility.SettleTile(map);
			}
			if (map.TileInfo.Layer.Def.isSpace)
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.Orbit, OpportunityType.Critical);
			}
			return map;
		}
		finally
		{
			DeepProfiler.End();
			if (!stepDebugger)
			{
				ClearWorkingData();
				mapBeingGenerated = null;
				gravship = null;
			}
			Current.ProgramState = programState;
			Rand.PopState();
		}
		static GenStepWithParams GetGenStepParms(GenStepDef x)
		{
			return new GenStepWithParams(x, default(GenStepParams));
		}
		static bool IsValidBiome(GenStepDef g)
		{
			return !Find.Scenario.parts.Any((ScenPart p) => typeof(ScenPart_DisableMapGen).IsAssignableFrom(p.def.scenPartClass) && p.def.genStep == g);
		}
	}

	public static bool DebugDoNextGenStep(Map map)
	{
		if (debugGenStepIndex >= tmpGenSteps.Count)
		{
			ClearDebugMode();
			return false;
		}
		GenStepWithParams genStepWithParams = tmpGenSteps[debugGenStepIndex];
		Log.Message("Doing gen step " + genStepWithParams.def.defName);
		Rand.PushState(Gen.HashCombineInt(debugSeed, GetSeedPart(tmpGenSteps, debugGenStepIndex)));
		try
		{
			GenStepParams parms = genStepWithParams.parms;
			if (gravship != null)
			{
				parms.gravship = gravship;
			}
			genStepWithParams.def.genStep.Generate(map, parms);
			genStepWithParams.def.genStep.PostMapInitialized(map, parms);
		}
		catch (Exception arg)
		{
			Log.Error($"Error stepping GenStep {genStepWithParams.def.defName}: {arg}");
		}
		Rand.PopState();
		map.FinalizeInit();
		debugGenStepIndex++;
		return true;
	}

	public static void ClearDebugMode()
	{
		debugMode = false;
		debugSeed = 0;
		debugGenStepIndex = 0;
	}

	public static void MapGeneratorPostInit(IEnumerable<GenStepWithParams> genStepDefs, Map map)
	{
		tmpGenSteps.Clear();
		tmpGenSteps.AddRange(from x in genStepDefs
			orderby x.def.order, x.def.index
			select x);
		tmpGenSteps.RemoveWhere((GenStepWithParams a) => tmpGenSteps.Any((GenStepWithParams b) => b.def.preventsGenSteps != null && b.def.preventsGenSteps.Contains(a.def)));
		for (int num = 0; num < tmpGenSteps.Count; num++)
		{
			GenStepParams parms = tmpGenSteps[num].parms;
			if (gravship != null)
			{
				parms.gravship = gravship;
			}
			tmpGenSteps[num].def.genStep.PostMapInitialized(map, parms);
		}
	}

	public static void GenerateContentsIntoMap(IEnumerable<GenStepWithParams> genStepDefs, Map map, int seed, bool stepDebugger = false)
	{
		ClearWorkingData();
		if (ModsConfig.OdysseyActive)
		{
			foreach (GenStepWithParams genStepDef in genStepDefs)
			{
				if (genStepDef.def == GenStepDefOf.GravshipMarker)
				{
					gravship = genStepDef.parms.gravship;
					break;
				}
			}
		}
		Rand.PushState();
		try
		{
			Rand.Seed = seed;
			RockNoises.Init(map);
			tmpGenSteps.Clear();
			tmpGenSteps.AddRange(from x in genStepDefs
				orderby x.def.order, x.def.index
				select x);
			tmpGenSteps.RemoveWhere((GenStepWithParams a) => tmpGenSteps.Any((GenStepWithParams b) => b.def.preventsGenSteps != null && b.def.preventsGenSteps.Contains(a.def)));
			if (stepDebugger)
			{
				return;
			}
			for (int num = 0; num < tmpGenSteps.Count; num++)
			{
				DeepProfiler.Start("GenStep - " + tmpGenSteps[num].def);
				try
				{
					GenStepParams parms = tmpGenSteps[num].parms;
					if (gravship != null)
					{
						parms.gravship = gravship;
					}
					Rand.PushState();
					Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(tmpGenSteps, num));
					tmpGenSteps[num].def.genStep.Generate(map, parms);
					if (map.pathing.IncrementalDirtyingDisabled)
					{
						Log.Error($"Genstep [{num}] {tmpGenSteps[num].def} ended with path incremental dirtying disabled, for safety reasons it must be reenabled before returning.");
						map.pathing.ReEnableIncrementalDirtying();
					}
					Rand.PopState();
				}
				catch (Exception arg)
				{
					Log.Error($"Error in GenStep: {arg}");
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}
		finally
		{
			if (!stepDebugger)
			{
				Rand.PopState();
				RockNoises.Reset();
			}
		}
	}

	private static void ClearWorkingData()
	{
		if (Prefs.DevMode && TryGetVar<List<CellRect>>("UsedRects", out var var))
		{
			cachedUsedRects = var.ToList();
		}
		foreach (KeyValuePair<string, object> datum in data)
		{
			datum.Deconstruct(out var _, out var value);
			if (value is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		data.Clear();
	}

	public static void DebugDraw()
	{
		if (!DebugViewSettings.drawUsedRects)
		{
			return;
		}
		float y = AltitudeLayer.MetaOverlays.AltitudeFor();
		foreach (CellRect cachedUsedRect in cachedUsedRects)
		{
			GenDraw.DrawLineBetween(new Vector3(cachedUsedRect.minX, y, cachedUsedRect.minZ), new Vector3(cachedUsedRect.minX, y, cachedUsedRect.maxZ + 1), SimpleColor.Red);
			GenDraw.DrawLineBetween(new Vector3(cachedUsedRect.maxX + 1, y, cachedUsedRect.minZ), new Vector3(cachedUsedRect.maxX + 1, y, cachedUsedRect.maxZ + 1), SimpleColor.Red);
			GenDraw.DrawLineBetween(new Vector3(cachedUsedRect.minX, y, cachedUsedRect.minZ), new Vector3(cachedUsedRect.maxX + 1, y, cachedUsedRect.minZ), SimpleColor.Red);
			GenDraw.DrawLineBetween(new Vector3(cachedUsedRect.minX, y, cachedUsedRect.maxZ + 1), new Vector3(cachedUsedRect.maxX + 1, y, cachedUsedRect.maxZ + 1), SimpleColor.Red);
		}
	}

	public static T GetVar<T>(string name)
	{
		if (data.TryGetValue(name, out var value))
		{
			return (T)value;
		}
		return default(T);
	}

	public static bool TryGetVar<T>(string name, out T var)
	{
		if (data.TryGetValue(name, out var value))
		{
			var = (T)value;
			return true;
		}
		var = default(T);
		return false;
	}

	public static T GetOrGenerateVar<T>(string name)
	{
		T var = GetVar<T>(name);
		if (var != null)
		{
			return var;
		}
		var = (T)Activator.CreateInstance(typeof(T));
		SetVar(name, var);
		return var;
	}

	public static void SetVar<T>(string name, T var)
	{
		data[name] = var;
	}

	public static MapGenFloatGrid FloatGridNamed(string name)
	{
		MapGenFloatGrid var = GetVar<MapGenFloatGrid>(name);
		if (var != null)
		{
			return var;
		}
		MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(mapBeingGenerated);
		SetVar(name, mapGenFloatGrid);
		return mapGenFloatGrid;
	}

	private static int GetSeedPart(List<GenStepWithParams> genSteps, int index)
	{
		int seedPart = genSteps[index].def.genStep.SeedPart;
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			if (tmpGenSteps[i].def.genStep.SeedPart == seedPart)
			{
				num++;
			}
		}
		return seedPart + num;
	}
}
