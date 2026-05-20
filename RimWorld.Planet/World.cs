using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet;

public sealed class World : IThingHolder, IExposable, IIncidentTarget, ILoadReferenceable, IDisposable
{
	public WorldInfo info = new WorldInfo();

	public List<WorldComponent> components = new List<WorldComponent>();

	public FactionManager factionManager;

	public IdeoManager ideoManager;

	public WorldPawns worldPawns;

	public WorldObjectsHolder worldObjects;

	public GameConditionManager gameConditionManager;

	public StoryState storyState;

	public WorldFeatures features;

	public WorldLandmarks landmarks;

	public List<PocketMapParent> pocketMaps = new List<PocketMapParent>();

	public WorldGrid grid;

	public WorldPathGrid pathGrid;

	public WorldRenderer renderer;

	public WorldInterface UI;

	public WorldDebugDrawer debugDrawer;

	public WorldDynamicDrawManager dynamicDrawManager;

	public WorldPathPool pathPool;

	public WorldReachability reachability;

	public WorldTilesInRandomOrder tilesInRandomOrder;

	public ConfiguredTicksAbsAtGameStartCache ticksAbsCache;

	public TileTemperaturesComp tileTemperatures;

	public WorldGenData genData;

	private List<ThingDef> allNaturalRockDefs;

	private static readonly List<ThingDef> tmpNaturalRockDefs = new List<ThingDef>();

	private static readonly List<PlanetTile> tmpNeighbors = new List<PlanetTile>();

	private static readonly List<Rot4> tTpOceanDirs = new List<Rot4>();

	public float PlanetCoverage => info.planetCoverage;

	public IThingHolder ParentHolder => null;

	public PlanetTile Tile => PlanetTile.Invalid;

	public StoryState StoryState => storyState;

	public GameConditionManager GameConditionManager => gameConditionManager;

	public float PlayerWealthForStoryteller
	{
		get
		{
			float num = 0f;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				num += maps[i].PlayerWealthForStoryteller;
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				num += caravans[j].PlayerWealthForStoryteller;
			}
			return num;
		}
	}

	public IEnumerable<Pawn> PlayerPawnsForStoryteller => PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;

	public FloatRange IncidentPointsRandomFactorRange => FloatRange.One;

	public int ConstantRandSeed => info.persistentRandomValue;

	public IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
	{
		yield return IncidentTargetTagDefOf.World;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref info, "info");
		Scribe_Deep.Look(ref grid, "grid");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (grid == null || !grid.HasWorldData)
			{
				WorldGenerator.GenerateWithoutWorldData(info.seedString);
			}
			else
			{
				WorldGenerator.GenerateFromScribe(info.seedString);
			}
		}
		else
		{
			ExposeComponents();
		}
	}

	public void ExposeComponents()
	{
		Scribe_Deep.Look(ref factionManager, "factionManager");
		Scribe_Deep.Look(ref ideoManager, "ideoManager");
		Scribe_Deep.Look(ref worldPawns, "worldPawns");
		Scribe_Deep.Look(ref worldObjects, "worldObjects");
		Scribe_Deep.Look(ref gameConditionManager, "gameConditionManager", this);
		Scribe_Deep.Look(ref storyState, "storyState", this);
		Scribe_Deep.Look(ref features, "features");
		Scribe_Deep.Look(ref landmarks, "landmarks");
		Scribe_Collections.Look(ref components, "components", LookMode.Deep, this);
		Scribe_Collections.Look(ref pocketMaps, "pocketMaps", LookMode.Deep);
		FillComponents();
		BackCompatibility.PostExposeData(this);
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			return;
		}
		if (pocketMaps == null)
		{
			pocketMaps = new List<PocketMapParent>();
		}
		if (landmarks == null)
		{
			landmarks = new WorldLandmarks();
		}
		if (!ModsConfig.OdysseyActive || !landmarks.landmarks.NullOrEmpty())
		{
			return;
		}
		foreach (Tile tile in grid.Surface.Tiles)
		{
			tile.mutatorsNullable?.Clear();
		}
		WorldGenStep_Mutators.AddMutatorsFromTile(grid.Surface);
		new WorldGenStep_Landmarks().GenerateFresh(info.seedString, grid.Surface);
	}

	public void ConstructComponents()
	{
		worldObjects = new WorldObjectsHolder();
		factionManager = new FactionManager();
		ideoManager = new IdeoManager();
		worldPawns = new WorldPawns();
		gameConditionManager = new GameConditionManager(this);
		storyState = new StoryState(this);
		renderer = new WorldRenderer();
		UI = new WorldInterface();
		debugDrawer = new WorldDebugDrawer();
		dynamicDrawManager = new WorldDynamicDrawManager();
		pathPool = new WorldPathPool();
		reachability = new WorldReachability();
		ticksAbsCache = new ConfiguredTicksAbsAtGameStartCache();
		tilesInRandomOrder = new WorldTilesInRandomOrder();
		components.Clear();
		FillComponents();
	}

	private void FillComponents()
	{
		components.RemoveAll((WorldComponent component) => component == null);
		foreach (Type item2 in typeof(WorldComponent).AllSubclassesNonAbstract())
		{
			if (GetComponent(item2) == null)
			{
				try
				{
					WorldComponent item = (WorldComponent)Activator.CreateInstance(item2, this);
					components.Add(item);
				}
				catch (Exception ex)
				{
					Log.Error("Could not instantiate a WorldComponent of type " + item2?.ToString() + ": " + ex);
				}
			}
		}
		tileTemperatures = GetComponent<TileTemperaturesComp>();
		genData = GetComponent<WorldGenData>();
	}

	public void FinalizeInit(bool fromLoad)
	{
		pathGrid.RecalculateAllLayersPathCosts();
		AmbientSoundManager.EnsureWorldAmbientSoundCreated();
		WorldComponentUtility.FinalizeInit(this, fromLoad);
	}

	public void WorldTick()
	{
		worldPawns.WorldPawnsTick();
		factionManager.FactionManagerTick();
		worldObjects.WorldObjectsHolderTick();
		debugDrawer.WorldDebugDrawerTick();
		pathGrid.WorldPathGridTick();
		WorldComponentUtility.WorldComponentTick(this);
		ideoManager.IdeoManagerTick();
	}

	public void WorldPostTick()
	{
		try
		{
			gameConditionManager.GameConditionManagerTick();
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
	}

	public void WorldUpdate()
	{
		bool worldRendered = WorldRendererUtility.WorldRendered;
		renderer.CheckActivateWorldCamera();
		if (worldRendered)
		{
			ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
			if (ModsConfig.OdysseyActive)
			{
				ExpandableLandmarksUtility.ExpandableLandmarksUpdate();
			}
			renderer.DrawWorldLayers();
			dynamicDrawManager.DrawDynamicWorldObjects();
			features.UpdateFeatures();
			NoiseDebugUI.RenderPlanetNoise();
		}
		WorldComponentUtility.WorldComponentUpdate(this);
		if (ModsConfig.BiotechActive)
		{
			try
			{
				CompDissolutionEffect_Goodwill.WorldUpdate();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			try
			{
				CompDissolutionEffect_Pollution.WorldUpdate();
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
		}
	}

	public void WorldOnGUI()
	{
		WorldComponentUtility.WorldComponentOnGUI(this);
	}

	public T GetComponent<T>() where T : WorldComponent
	{
		for (int i = 0; i < components.Count; i++)
		{
			if (components[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public WorldComponent GetComponent(Type type)
	{
		for (int i = 0; i < components.Count; i++)
		{
			if (type.IsInstanceOfType(components[i]))
			{
				return components[i];
			}
		}
		return null;
	}

	public float? CoastAngleAt(PlanetTile tile, BiomeDef waterBiome)
	{
		tmpNeighbors.Clear();
		Find.World.grid.GetTileNeighbors(tile, tmpNeighbors);
		IEnumerable<PlanetTile> source = tmpNeighbors.Where((PlanetTile t) => t.Tile.PrimaryBiome == waterBiome);
		if (!source.Any())
		{
			return null;
		}
		float num = GenMath.MeanAngle(source.Select((PlanetTile t) => Find.WorldGrid.GetHeadingFromTo(t, tile)).ToList());
		return (450f - num) % 360f;
	}

	public Rot4 CoastDirectionAt(PlanetTile tile)
	{
		if (!tile.Valid)
		{
			return Rot4.Invalid;
		}
		if (!tile.Tile.PrimaryBiome.canBuildBase)
		{
			return Rot4.Invalid;
		}
		tTpOceanDirs.Clear();
		grid.GetTileNeighbors(tile, tmpNeighbors);
		int i = 0;
		for (int count = tmpNeighbors.Count; i < count; i++)
		{
			if (grid[tmpNeighbors[i]].PrimaryBiome == BiomeDefOf.Ocean)
			{
				Rot4 rotFromTo = grid.GetRotFromTo(tile, tmpNeighbors[i]);
				if (!tTpOceanDirs.Contains(rotFromTo))
				{
					tTpOceanDirs.Add(rotFromTo);
				}
			}
		}
		if (tTpOceanDirs.Count == 0)
		{
			return Rot4.Invalid;
		}
		Rand.PushState();
		Rand.Seed = tile.GetHashCode();
		int index = Rand.Range(0, tTpOceanDirs.Count);
		Rand.PopState();
		return tTpOceanDirs[index];
	}

	public Rot4 LakeDirectionAt(PlanetTile tile)
	{
		if (!tile.Valid)
		{
			return Rot4.Invalid;
		}
		if (!tile.Tile.PrimaryBiome.canBuildBase)
		{
			return Rot4.Invalid;
		}
		tTpOceanDirs.Clear();
		grid.GetTileNeighbors(tile, tmpNeighbors);
		int i = 0;
		for (int count = tmpNeighbors.Count; i < count; i++)
		{
			if (grid[tmpNeighbors[i]].PrimaryBiome == BiomeDefOf.Lake)
			{
				Rot4 rotFromTo = grid.GetRotFromTo(tile, tmpNeighbors[i]);
				if (!tTpOceanDirs.Contains(rotFromTo))
				{
					tTpOceanDirs.Add(rotFromTo);
				}
			}
		}
		if (tTpOceanDirs.Count == 0)
		{
			return Rot4.Invalid;
		}
		Rand.PushState();
		Rand.Seed = tile.GetHashCode();
		int index = Rand.Range(0, tTpOceanDirs.Count);
		Rand.PopState();
		return tTpOceanDirs[index];
	}

	public bool HasCaves(PlanetTile tile)
	{
		return tile.Tile.Mutators.Any((TileMutatorDef m) => m.IsCave);
	}

	public IEnumerable<ThingDef> NaturalRockTypesIn(PlanetTile tile)
	{
		if (tile.Valid)
		{
			List<ThingDef> forceRockTypes = tile.Tile.PrimaryBiome.forceRockTypes;
			if (forceRockTypes != null)
			{
				return forceRockTypes;
			}
		}
		Rand.PushState();
		Rand.Seed = tile.GetHashCode();
		if (allNaturalRockDefs == null)
		{
			allNaturalRockDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.IsNonResourceNaturalRock).ToList();
		}
		int num = Rand.RangeInclusive(2, 3);
		if (num > allNaturalRockDefs.Count)
		{
			num = allNaturalRockDefs.Count;
		}
		tmpNaturalRockDefs.Clear();
		tmpNaturalRockDefs.AddRange(allNaturalRockDefs);
		List<ThingDef> result = tmpNaturalRockDefs.Where((ThingDef def) => RockAllowedInBiome(def, tile)).TakeRandomDistinct(num);
		Rand.PopState();
		return result;
	}

	private static bool RockAllowedInBiome(ThingDef def, PlanetTile tile)
	{
		if (def.building.biomeSpecific)
		{
			if (tile.Valid)
			{
				return tile.Tile.PrimaryBiome.extraRockTypes.NotNullAndContains(def);
			}
			return false;
		}
		return true;
	}

	public bool Impassable(PlanetTile tileID)
	{
		return !pathGrid.Passable(tileID);
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return null;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		List<WorldObject> allWorldObjects = worldObjects.AllWorldObjects;
		for (int i = 0; i < allWorldObjects.Count; i++)
		{
			if (allWorldObjects[i] is IThingHolder item)
			{
				outChildren.Add(item);
			}
			List<WorldObjectComp> allComps = allWorldObjects[i].AllComps;
			for (int j = 0; j < allComps.Count; j++)
			{
				if (allComps[j] is IThingHolder item2)
				{
					outChildren.Add(item2);
				}
			}
		}
		for (int k = 0; k < components.Count; k++)
		{
			if (components[k] is IThingHolder item3)
			{
				outChildren.Add(item3);
			}
		}
	}

	public string GetUniqueLoadID()
	{
		return "World";
	}

	public override string ToString()
	{
		return "World-" + info.name;
	}

	public void Dispose()
	{
		grid?.Dispose();
	}
}
