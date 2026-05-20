using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public sealed class Map : IIncidentTarget, ILoadReferenceable, IThingHolder, IExposable, IDisposable
{
	public MapFileCompressor compressor;

	private List<Thing> loadedFullThings;

	public MapGeneratorDef generatorDef;

	public int uniqueID = -1;

	public int generationTick;

	public bool wasSpawnedViaGravShipLanding;

	private Color? fogOfWarColor;

	private OrbitalDebrisDef orbitalDebris;

	private int generatedId;

	public MapInfo info = new MapInfo();

	public MapEvents events;

	public List<MapComponent> components = new List<MapComponent>();

	public ThingOwner spawnedThings;

	public CellIndices cellIndices;

	public ListerThings listerThings;

	public ListerBuildings listerBuildings;

	public MapPawns mapPawns;

	public DynamicDrawManager dynamicDrawManager;

	public MapDrawer mapDrawer;

	public PawnDestinationReservationManager pawnDestinationReservationManager;

	public TooltipGiverList tooltipGiverList;

	public ReservationManager reservationManager;

	public EnrouteManager enrouteManager;

	public PhysicalInteractionReservationManager physicalInteractionReservationManager;

	public DesignationManager designationManager;

	public LordManager lordManager;

	public PassingShipManager passingShipManager;

	public HaulDestinationManager haulDestinationManager;

	public DebugCellDrawer debugDrawer;

	public GameConditionManager gameConditionManager;

	public WeatherManager weatherManager;

	public ZoneManager zoneManager;

	public PlanManager planManager;

	public ResourceCounter resourceCounter;

	public MapTemperature mapTemperature;

	public TemperatureVacuumCache TemperatureVacuumCache;

	public AreaManager areaManager;

	public AttackTargetsCache attackTargetsCache;

	public AttackTargetReservationManager attackTargetReservationManager;

	public VoluntarilyJoinableLordsStarter lordsStarter;

	public FleckManager flecks;

	public DeferredSpawner deferredSpawner;

	public ThingGrid thingGrid;

	public CoverGrid coverGrid;

	public EdificeGrid edificeGrid;

	public BlueprintGrid blueprintGrid;

	public FogGrid fogGrid;

	public RegionGrid regionGrid;

	public GlowGrid glowGrid;

	public TerrainGrid terrainGrid;

	public Pathing pathing;

	public RoofGrid roofGrid;

	public FertilityGrid fertilityGrid;

	public SnowGrid snowGrid;

	public DeepResourceGrid deepResourceGrid;

	public ExitMapGrid exitMapGrid;

	public AvoidGrid avoidGrid;

	public GasGrid gasGrid;

	public PollutionGrid pollutionGrid;

	public SubstructureGrid substructureGrid;

	public WaterBodyTracker waterBodyTracker;

	public SandGrid sandGrid;

	public LinkGrid linkGrid;

	public PowerNetManager powerNetManager;

	public PowerNetGrid powerNetGrid;

	public RegionMaker regionMaker;

	public PathFinder pathFinder;

	public PawnPathPool pawnPathPool;

	public RegionAndRoomUpdater regionAndRoomUpdater;

	public RegionLinkDatabase regionLinkDatabase;

	public MoteCounter moteCounter;

	public GatherSpotLister gatherSpotLister;

	public WindManager windManager;

	public ListerBuildingsRepairable listerBuildingsRepairable;

	public ListerHaulables listerHaulables;

	public ListerMergeables listerMergeables;

	public ListerArtificialBuildingsForMeditation listerArtificialBuildingsForMeditation;

	public ListerBuldingOfDefInProximity listerBuldingOfDefInProximity;

	public ListerBuildingWithTagInProximity listerBuildingWithTagInProximity;

	public ListerFilthInHomeArea listerFilthInHomeArea;

	public Reachability reachability;

	public ItemAvailability itemAvailability;

	public AutoBuildRoofAreaSetter autoBuildRoofAreaSetter;

	public RoofCollapseBufferResolver roofCollapseBufferResolver;

	public RoofCollapseBuffer roofCollapseBuffer;

	public WildAnimalSpawner wildAnimalSpawner;

	public WildPlantSpawner wildPlantSpawner;

	public SteadyEnvironmentEffects steadyEnvironmentEffects;

	public TempTerrainManager tempTerrain;

	public FreezeManager freezeManager;

	public SkyManager skyManager;

	public OverlayDrawer overlayDrawer;

	public FloodFiller floodFiller;

	public WeatherDecider weatherDecider;

	public FireWatcher fireWatcher;

	public DangerWatcher dangerWatcher;

	public DamageWatcher damageWatcher;

	public StrengthWatcher strengthWatcher;

	public WealthWatcher wealthWatcher;

	public RegionDirtyer regionDirtyer;

	public MapCellsInRandomOrder cellsInRandomOrder;

	public RememberedCameraPos rememberedCameraPos;

	public MineStrikeManager mineStrikeManager;

	public StoryState storyState;

	public RoadInfo roadInfo;

	public WaterInfo waterInfo;

	public RetainedCaravanData retainedCaravanData;

	public TemporaryThingDrawer temporaryThingDrawer;

	public AnimalPenManager animalPenManager;

	public MapPlantGrowthRateCalculator plantGrowthRateCalculator;

	public AutoSlaughterManager autoSlaughterManager;

	public TreeDestructionTracker treeDestructionTracker;

	public StorageGroupManager storageGroups;

	public EffecterMaintainer effecterMaintainer;

	public PostTickVisuals postTickVisuals;

	public List<LayoutStructureSketch> layoutStructureSketches = new List<LayoutStructureSketch>();

	public ThingListChangedCallbacks thingListChangedCallbacks = new ThingListChangedCallbacks();

	public List<CellRect> landingBlockers = new List<CellRect>();

	public Tile pocketTileInfo;

	public const string ThingSaveKey = "thing";

	[TweakValue("Graphics_Shadow", 0f, 100f)]
	private static bool AlwaysRedrawShadows;

	private MixedBiomeMapComponent mixedBiomeComp;

	public int Index => Find.Maps.IndexOf(this);

	public IntVec3 Size => info.Size;

	public IntVec3 Center => new IntVec3(Size.x / 2, 0, Size.z / 2);

	public Faction ParentFaction => info.parent?.Faction;

	public int Area => Size.x * Size.z;

	public IThingHolder ParentHolder => info.parent;

	public bool DrawMapClippers => !generatorDef.disableMapClippers;

	public bool CanEverExit
	{
		get
		{
			if (!info.isPocketMap)
			{
				return Biome.canExitMap;
			}
			return false;
		}
	}

	public Color? FogOfWarColor
	{
		get
		{
			return fogOfWarColor ?? Biome.fogOfWarColor;
		}
		set
		{
			fogOfWarColor = value;
		}
	}

	public OrbitalDebrisDef OrbitalDebris
	{
		get
		{
			return orbitalDebris ?? Biome.orbitalDebris;
		}
		set
		{
			orbitalDebris = value;
		}
	}

	public Material MapEdgeMaterial
	{
		get
		{
			if (ModsConfig.AnomalyActive && generatorDef == MapGeneratorDefOf.MetalHell)
			{
				return MapEdgeClipDrawer.ClipMatMetalhell;
			}
			WorldObject parent = Parent;
			if (parent != null && parent.def.MapEdgeMaterial != null)
			{
				return parent.def.MapEdgeMaterial;
			}
			if (generatorDef.mapClipperShader != null)
			{
				if (!generatorDef.mapClipperTexturePath.NullOrEmpty())
				{
					return MaterialPool.MatFrom(generatorDef.mapClipperTexturePath, generatorDef.mapClipperShader.Shader);
				}
				return MaterialPool.MatFrom(generatorDef.mapClipperShader.Shader);
			}
			return MapEdgeClipDrawer.ClipMat;
		}
	}

	public bool Disposed { get; private set; }

	public IEnumerable<IntVec3> AllCells
	{
		get
		{
			for (int z = 0; z < Size.z; z++)
			{
				for (int y = 0; y < Size.y; y++)
				{
					for (int x = 0; x < Size.x; x++)
					{
						yield return new IntVec3(x, y, z);
					}
				}
			}
		}
	}

	public bool IsPlayerHome
	{
		get
		{
			if (!wasSpawnedViaGravShipLanding && (info?.parent == null || info.parent.Faction != Faction.OfPlayer || !info.parent.def.canBePlayerHome))
			{
				return GravshipUtility.PlayerHasGravEngine(this);
			}
			return true;
		}
	}

	public bool TreatAsPlayerHomeForThreatPoints
	{
		get
		{
			if (IsPlayerHome)
			{
				return true;
			}
			if (info.parent != null && info.parent.def.treatAsPlayerHome)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsTempIncidentMap => info.parent.def.isTempIncidentMapOwner;

	public PlanetTile Tile => info.Tile;

	public Tile TileInfo
	{
		get
		{
			if (!IsPocketMap)
			{
				return Find.WorldGrid[Tile];
			}
			return pocketTileInfo;
		}
	}

	public BiomeDef Biome => TileInfo.PrimaryBiome;

	public IEnumerable<BiomeDef> Biomes => TileInfo.Biomes;

	public MixedBiomeMapComponent MixedBiomeComp => mixedBiomeComp ?? (mixedBiomeComp = GetComponent<MixedBiomeMapComponent>());

	public bool IsStartingMap => Find.GameInfo.startingTile == Tile;

	public bool IsPocketMap => info.isPocketMap;

	public StoryState StoryState => storyState;

	public GameConditionManager GameConditionManager => gameConditionManager;

	public float PlayerWealthForStoryteller
	{
		get
		{
			if (TreatAsPlayerHomeForThreatPoints)
			{
				if (Find.Storyteller.difficulty.fixedWealthMode)
				{
					return StorytellerUtility.FixedWealthModeMapWealthFromTimeCurve.Evaluate(AgeInDays * Find.Storyteller.difficulty.fixedWealthTimeFactor);
				}
				return wealthWatcher.WealthItems + wealthWatcher.WealthBuildings * 0.5f + wealthWatcher.WealthPawns;
			}
			float num = 0f;
			foreach (Pawn item in mapPawns.PawnsInFaction(Faction.OfPlayer))
			{
				if (item.IsFreeColonist)
				{
					num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(item);
				}
				if (item.IsAnimal)
				{
					num += item.MarketValue;
				}
			}
			return num;
		}
	}

	public IEnumerable<Pawn> PlayerPawnsForStoryteller => mapPawns.PawnsInFaction(Faction.OfPlayer);

	public FloatRange IncidentPointsRandomFactorRange => FloatRange.One;

	public MapParent Parent => info.parent;

	public PocketMapParent PocketMapParent
	{
		get
		{
			if (!IsPocketMap)
			{
				return null;
			}
			return Parent as PocketMapParent;
		}
	}

	public IEnumerable<Map> ChildPocketMaps
	{
		get
		{
			foreach (PocketMapParent pocketMap in Find.World.pocketMaps)
			{
				if (pocketMap.sourceMap == this)
				{
					yield return pocketMap.Map;
				}
			}
		}
	}

	public float AgeInDays => (float)(Find.TickManager.TicksGame - generationTick) / 60000f;

	public bool AnyBuildingBlockingMapRemoval
	{
		get
		{
			if (ModsConfig.OdysseyActive)
			{
				if (listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
				{
					return true;
				}
				if (listerThings.AnyThingWithDef(ThingDefOf.GravEngine))
				{
					return true;
				}
			}
			return false;
		}
	}

	public int NextGenSeed => HashCode.Combine(TileInfo.tile.Valid ? TileInfo.tile.GetHashCode() : uniqueID, generatedId++, Find.World.info.Seed);

	public int ConstantRandSeed => uniqueID ^ 0xFDA252;

	public IEnumerator<IntVec3> GetEnumerator()
	{
		foreach (IntVec3 allCell in AllCells)
		{
			yield return allCell;
		}
	}

	public IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
	{
		return info.parent?.IncidentTargetTags() ?? Enumerable.Empty<IncidentTargetTagDef>();
	}

	public void ConstructComponents()
	{
		spawnedThings = new ThingOwner<Thing>(this);
		cellIndices = new CellIndices(this);
		listerThings = new ListerThings(ListerThingsUse.Global, thingListChangedCallbacks);
		listerBuildings = new ListerBuildings();
		mapPawns = new MapPawns(this);
		dynamicDrawManager = new DynamicDrawManager(this);
		mapDrawer = new MapDrawer(this);
		tooltipGiverList = new TooltipGiverList();
		pawnDestinationReservationManager = new PawnDestinationReservationManager();
		reservationManager = new ReservationManager(this);
		enrouteManager = new EnrouteManager(this);
		physicalInteractionReservationManager = new PhysicalInteractionReservationManager();
		designationManager = new DesignationManager(this);
		lordManager = new LordManager(this);
		debugDrawer = new DebugCellDrawer();
		passingShipManager = new PassingShipManager(this);
		haulDestinationManager = new HaulDestinationManager(this);
		gameConditionManager = new GameConditionManager(this);
		weatherManager = new WeatherManager(this);
		zoneManager = new ZoneManager(this);
		planManager = new PlanManager(this);
		resourceCounter = new ResourceCounter(this);
		mapTemperature = new MapTemperature(this);
		TemperatureVacuumCache = new TemperatureVacuumCache(this);
		areaManager = new AreaManager(this);
		attackTargetsCache = new AttackTargetsCache(this);
		attackTargetReservationManager = new AttackTargetReservationManager(this);
		lordsStarter = new VoluntarilyJoinableLordsStarter(this);
		flecks = new FleckManager(this);
		deferredSpawner = new DeferredSpawner(this);
		thingGrid = new ThingGrid(this);
		coverGrid = new CoverGrid(this);
		edificeGrid = new EdificeGrid(this);
		blueprintGrid = new BlueprintGrid(this);
		fogGrid = new FogGrid(this);
		glowGrid = new GlowGrid(this);
		regionGrid = new RegionGrid(this);
		terrainGrid = new TerrainGrid(this);
		pathing = new Pathing(this);
		roofGrid = new RoofGrid(this);
		fertilityGrid = new FertilityGrid(this);
		snowGrid = new SnowGrid(this);
		gasGrid = new GasGrid(this);
		pollutionGrid = new PollutionGrid(this);
		deepResourceGrid = new DeepResourceGrid(this);
		exitMapGrid = new ExitMapGrid(this);
		avoidGrid = new AvoidGrid(this);
		linkGrid = new LinkGrid(this);
		powerNetManager = new PowerNetManager(this);
		powerNetGrid = new PowerNetGrid(this);
		regionMaker = new RegionMaker(this);
		pathFinder = new PathFinder(this);
		pawnPathPool = new PawnPathPool(this);
		regionAndRoomUpdater = new RegionAndRoomUpdater(this);
		regionLinkDatabase = new RegionLinkDatabase();
		moteCounter = new MoteCounter();
		gatherSpotLister = new GatherSpotLister();
		windManager = new WindManager(this);
		listerBuildingsRepairable = new ListerBuildingsRepairable();
		listerHaulables = new ListerHaulables(this);
		listerMergeables = new ListerMergeables(this);
		listerFilthInHomeArea = new ListerFilthInHomeArea(this);
		listerArtificialBuildingsForMeditation = new ListerArtificialBuildingsForMeditation(this);
		listerBuldingOfDefInProximity = new ListerBuldingOfDefInProximity(this);
		listerBuildingWithTagInProximity = new ListerBuildingWithTagInProximity(this);
		reachability = new Reachability(this);
		itemAvailability = new ItemAvailability(this);
		autoBuildRoofAreaSetter = new AutoBuildRoofAreaSetter(this);
		roofCollapseBufferResolver = new RoofCollapseBufferResolver(this);
		roofCollapseBuffer = new RoofCollapseBuffer();
		wildAnimalSpawner = new WildAnimalSpawner(this);
		wildPlantSpawner = new WildPlantSpawner(this);
		steadyEnvironmentEffects = new SteadyEnvironmentEffects(this);
		tempTerrain = new TempTerrainManager(this);
		skyManager = new SkyManager(this);
		overlayDrawer = new OverlayDrawer();
		floodFiller = new FloodFiller(this);
		weatherDecider = new WeatherDecider(this);
		fireWatcher = new FireWatcher(this);
		dangerWatcher = new DangerWatcher(this);
		damageWatcher = new DamageWatcher();
		strengthWatcher = new StrengthWatcher(this);
		wealthWatcher = new WealthWatcher(this);
		regionDirtyer = new RegionDirtyer(this);
		cellsInRandomOrder = new MapCellsInRandomOrder(this);
		rememberedCameraPos = new RememberedCameraPos(this);
		mineStrikeManager = new MineStrikeManager();
		storyState = new StoryState(this);
		retainedCaravanData = new RetainedCaravanData(this);
		temporaryThingDrawer = new TemporaryThingDrawer();
		animalPenManager = new AnimalPenManager(this);
		plantGrowthRateCalculator = new MapPlantGrowthRateCalculator();
		autoSlaughterManager = new AutoSlaughterManager(this);
		treeDestructionTracker = new TreeDestructionTracker(this);
		storageGroups = new StorageGroupManager(this);
		effecterMaintainer = new EffecterMaintainer(this);
		postTickVisuals = new PostTickVisuals(this);
		if (ModsConfig.OdysseyActive)
		{
			substructureGrid = new SubstructureGrid(this);
			waterBodyTracker = new WaterBodyTracker(this);
			freezeManager = new FreezeManager(this);
			sandGrid = new SandGrid(this);
		}
		components.Clear();
		FillComponents();
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			events = new MapEvents(this);
		}
		Scribe_Values.Look(ref uniqueID, "uniqueID", -1);
		Scribe_Values.Look(ref generationTick, "generationTick", 0);
		Scribe_Values.Look(ref wasSpawnedViaGravShipLanding, "wasSpawnedViaGravShipLanding", defaultValue: false);
		Scribe_Values.Look(ref fogOfWarColor, "fogOfWarColor");
		Scribe_Values.Look(ref generatedId, "generatedId", 0);
		Scribe_Defs.Look(ref orbitalDebris, "orbitalDebris");
		Scribe_Defs.Look(ref generatorDef, "generatorDef");
		Scribe_Deep.Look(ref pocketTileInfo, "pocketTileInfo");
		Scribe_Deep.Look(ref info, "mapInfo");
		Scribe_Collections.Look(ref layoutStructureSketches, "layoutStructureSketches", LookMode.Deep);
		Scribe_Collections.Look(ref landingBlockers, "landingBlockers", LookMode.Undefined);
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			compressor = new MapFileCompressor(this);
			compressor.BuildCompressedString();
			ExposeComponents();
			compressor.ExposeData();
			HashSet<string> hashSet = new HashSet<string>();
			if (Scribe.EnterNode("things"))
			{
				try
				{
					foreach (Thing allThing in listerThings.AllThings)
					{
						try
						{
							if (allThing.def.isSaveable && !allThing.IsSaveCompressible())
							{
								if (!hashSet.Add(allThing.ThingID))
								{
									Log.Error("Saving Thing with already-used ID " + allThing.ThingID);
								}
								else
								{
									hashSet.Add(allThing.ThingID);
								}
								Thing target = allThing;
								Scribe_Deep.Look(ref target, "thing");
							}
						}
						catch (OutOfMemoryException)
						{
							throw;
						}
						catch (Exception arg)
						{
							Log.Error($"Exception saving {allThing}: {arg}");
						}
					}
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			else
			{
				Log.Error("Could not enter the things node while saving.");
			}
			compressor = null;
		}
		else
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				ConstructComponents();
				regionAndRoomUpdater.Enabled = false;
				compressor = new MapFileCompressor(this);
			}
			else if (Scribe.mode == LoadSaveMode.PostLoadInit && landingBlockers == null)
			{
				landingBlockers = new List<CellRect>();
			}
			ExposeComponents();
			DeepProfiler.Start("Load compressed things");
			compressor.ExposeData();
			DeepProfiler.End();
			DeepProfiler.Start("Load non-compressed things");
			Scribe_Collections.Look(ref loadedFullThings, "things", LookMode.Deep);
			DeepProfiler.End();
		}
		BackCompatibility.PostExposeData(this);
	}

	private void FillComponents()
	{
		components.RemoveAll((MapComponent component) => component == null);
		foreach (Type item3 in typeof(MapComponent).AllSubclassesNonAbstract())
		{
			if (!typeof(CustomMapComponent).IsAssignableFrom(item3) && GetComponent(item3) == null)
			{
				try
				{
					MapComponent item = (MapComponent)Activator.CreateInstance(item3, this);
					components.Add(item);
				}
				catch (Exception ex)
				{
					Log.Error("Could not instantiate a MapComponent of type " + item3?.ToString() + ": " + ex);
				}
			}
		}
		if (generatorDef?.customMapComponents != null)
		{
			foreach (Type customMapComponent in generatorDef.customMapComponents)
			{
				if (GetComponent(customMapComponent) == null)
				{
					try
					{
						MapComponent item2 = (MapComponent)Activator.CreateInstance(customMapComponent, this);
						components.Add(item2);
					}
					catch (Exception ex2)
					{
						Log.Error("Could not instantiate a MapComponent of type " + customMapComponent?.ToString() + ": " + ex2);
					}
				}
			}
		}
		roadInfo = GetComponent<RoadInfo>();
		waterInfo = GetComponent<WaterInfo>();
	}

	public void FinalizeLoading()
	{
		regionAndRoomUpdater.Enabled = true;
		List<Thing> list = compressor.ThingsToSpawnAfterLoad().ToList();
		compressor = null;
		DeepProfiler.Start("Merge compressed and non-compressed thing lists");
		List<Thing> list2 = new List<Thing>(loadedFullThings.Count + list.Count);
		foreach (Thing item in loadedFullThings.Concat(list))
		{
			list2.Add(item);
		}
		loadedFullThings.Clear();
		DeepProfiler.End();
		DeepProfiler.Start("Spawn everything into the map");
		BackCompatibility.PreCheckSpawnBackCompatibleThingAfterLoading(this);
		foreach (Thing item2 in list2)
		{
			if (item2 is Building)
			{
				continue;
			}
			try
			{
				if (!BackCompatibility.CheckSpawnBackCompatibleThingAfterLoading(item2, this))
				{
					GenSpawn.Spawn(item2, item2.Position, this, item2.Rotation, WipeMode.FullRefund, respawningAfterLoad: true);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception spawning loaded thing " + item2.ToStringSafe() + ": " + ex);
			}
		}
		foreach (Building item3 in from t in list2.OfType<Building>()
			orderby t.def.size.Magnitude
			select t)
		{
			try
			{
				GenSpawn.SpawnBuildingAsPossible(item3, this, respawningAfterLoad: true);
			}
			catch (Exception ex2)
			{
				Log.Error("Exception spawning loaded thing " + item3.ToStringSafe() + ": " + ex2);
			}
		}
		BackCompatibility.PostCheckSpawnBackCompatibleThingAfterLoading(this);
		DeepProfiler.End();
		FinalizeInit();
	}

	public void FinalizeInit()
	{
		DeepProfiler.Start("Finalize geometry");
		pathing.RecalculateAllPerceivedPathCosts();
		regionAndRoomUpdater.Enabled = true;
		regionAndRoomUpdater.RebuildAllRegionsAndRooms();
		powerNetManager.UpdatePowerNetsAndConnections_First();
		TemperatureVacuumCache.TemperatureVacuumSaveLoad.ApplyLoadedDataToRegions();
		avoidGrid.Regenerate();
		animalPenManager.RebuildAllPens();
		plantGrowthRateCalculator.BuildFor(this);
		gasGrid.RecalculateEverHadGas();
		DeepProfiler.End();
		DeepProfiler.Start("Thing.PostMapInit()");
		foreach (Thing item in listerThings.AllThings.ToList())
		{
			try
			{
				item.PostMapInit();
			}
			catch (Exception ex)
			{
				Log.Error("Error in PostMapInit() for " + item.ToStringSafe() + ": " + ex);
			}
		}
		DeepProfiler.End();
		DeepProfiler.Start("listerFilthInHomeArea.RebuildAll()");
		listerFilthInHomeArea.RebuildAll();
		DeepProfiler.End();
		if (ModsConfig.OdysseyActive)
		{
			GetComponent<VacuumComponent>().SetDrawerDirty();
		}
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			mapDrawer.RegenerateEverythingNow();
		});
		DeepProfiler.Start("resourceCounter.UpdateResourceCounts()");
		resourceCounter.UpdateResourceCounts();
		DeepProfiler.End();
		DeepProfiler.Start("wealthWatcher.ForceRecount()");
		wealthWatcher.ForceRecount(allowDuringInit: true);
		DeepProfiler.End();
		if (ModsConfig.OdysseyActive)
		{
			using (new ProfilerBlock("WaterBodyTracker.ConstructBodies()"))
			{
				waterBodyTracker?.ConstructBodies();
			}
		}
		MapComponentUtility.FinalizeInit(this);
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			Find.MusicManagerPlay.CheckTransitions();
		});
	}

	private void ExposeComponents()
	{
		Scribe_Deep.Look(ref weatherManager, "weatherManager", this);
		Scribe_Deep.Look(ref reservationManager, "reservationManager", this);
		Scribe_Deep.Look(ref enrouteManager, "enrouteManager", this);
		Scribe_Deep.Look(ref physicalInteractionReservationManager, "physicalInteractionReservationManager");
		Scribe_Deep.Look(ref planManager, "planManager", this);
		Scribe_Deep.Look(ref designationManager, "designationManager", this);
		Scribe_Deep.Look(ref pawnDestinationReservationManager, "pawnDestinationReservationManager");
		Scribe_Deep.Look(ref lordManager, "lordManager", this);
		Scribe_Deep.Look(ref passingShipManager, "visitorManager", this);
		Scribe_Deep.Look(ref gameConditionManager, "gameConditionManager", this);
		Scribe_Deep.Look(ref fogGrid, "fogGrid", this);
		Scribe_Deep.Look(ref roofGrid, "roofGrid", this);
		Scribe_Deep.Look(ref terrainGrid, "terrainGrid", this);
		Scribe_Deep.Look(ref zoneManager, "zoneManager", this);
		Scribe_Deep.Look(ref TemperatureVacuumCache, "temperatureCache", this);
		Scribe_Deep.Look(ref snowGrid, "snowGrid", this);
		Scribe_Deep.Look(ref gasGrid, "gasGrid", this);
		Scribe_Deep.Look(ref pollutionGrid, "pollutionGrid", this);
		Scribe_Deep.Look(ref waterBodyTracker, "waterBodyTracker", this);
		Scribe_Deep.Look(ref areaManager, "areaManager", this);
		Scribe_Deep.Look(ref lordsStarter, "lordsStarter", this);
		Scribe_Deep.Look(ref attackTargetReservationManager, "attackTargetReservationManager", this);
		Scribe_Deep.Look(ref deepResourceGrid, "deepResourceGrid", this);
		Scribe_Deep.Look(ref weatherDecider, "weatherDecider", this);
		Scribe_Deep.Look(ref damageWatcher, "damageWatcher");
		Scribe_Deep.Look(ref rememberedCameraPos, "rememberedCameraPos", this);
		Scribe_Deep.Look(ref mineStrikeManager, "mineStrikeManager");
		Scribe_Deep.Look(ref retainedCaravanData, "retainedCaravanData", this);
		Scribe_Deep.Look(ref storyState, "storyState", this);
		Scribe_Deep.Look(ref tempTerrain, "tempTerrain", this);
		Scribe_Deep.Look(ref wildPlantSpawner, "wildPlantSpawner", this);
		Scribe_Deep.Look(ref temporaryThingDrawer, "temporaryThingDrawer");
		Scribe_Deep.Look(ref flecks, "flecks", this);
		Scribe_Deep.Look(ref deferredSpawner, "deferredSpawner", this);
		Scribe_Deep.Look(ref autoSlaughterManager, "autoSlaughterManager", this);
		Scribe_Deep.Look(ref treeDestructionTracker, "treeDestructionTracker", this);
		Scribe_Deep.Look(ref storageGroups, "storageGroups", this);
		Scribe_Deep.Look(ref sandGrid, "sandGrid", this);
		Scribe_Collections.Look(ref components, "components", LookMode.Deep, this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (planManager == null)
			{
				planManager = new PlanManager(this);
			}
			if (ModsConfig.BiotechActive && pollutionGrid == null)
			{
				pollutionGrid = new PollutionGrid(this);
			}
			if (ModsConfig.OdysseyActive)
			{
				if (sandGrid == null)
				{
					sandGrid = new SandGrid(this);
				}
				if (substructureGrid == null)
				{
					substructureGrid = new SubstructureGrid(this);
				}
				if (waterBodyTracker == null)
				{
					waterBodyTracker = new WaterBodyTracker(this);
				}
				if (freezeManager == null)
				{
					freezeManager = new FreezeManager(this);
				}
			}
		}
		FillComponents();
		BackCompatibility.PostExposeData(this);
	}

	public void MapPreTick()
	{
		itemAvailability.Tick();
		listerHaulables.ListerHaulablesTick();
		try
		{
			autoBuildRoofAreaSetter.AutoBuildRoofAreaSetterTick_First();
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		roofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
		windManager.WindManagerTick();
		try
		{
			mapTemperature.MapTemperatureTick();
		}
		catch (Exception ex2)
		{
			Log.Error(ex2.ToString());
		}
		temporaryThingDrawer.Tick();
		try
		{
			pathFinder.PathFinderTick();
		}
		catch (Exception ex3)
		{
			Log.Error(ex3.ToString());
		}
	}

	public void MapPostTick()
	{
		try
		{
			wildAnimalSpawner.WildAnimalSpawnerTick();
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		try
		{
			wildPlantSpawner.WildPlantSpawnerTick();
		}
		catch (Exception ex2)
		{
			Log.Error(ex2.ToString());
		}
		try
		{
			powerNetManager.PowerNetsTick();
		}
		catch (Exception ex3)
		{
			Log.Error(ex3.ToString());
		}
		try
		{
			steadyEnvironmentEffects.SteadyEnvironmentEffectsTick();
		}
		catch (Exception ex4)
		{
			Log.Error(ex4.ToString());
		}
		try
		{
			tempTerrain.Tick();
		}
		catch (Exception ex5)
		{
			Log.Error(ex5.ToString());
		}
		try
		{
			gasGrid.Tick();
		}
		catch (Exception ex6)
		{
			Log.Error(ex6.ToString());
		}
		if (ModsConfig.BiotechActive)
		{
			try
			{
				pollutionGrid.PollutionTick();
			}
			catch (Exception ex7)
			{
				Log.Error(ex7.ToString());
			}
		}
		try
		{
			deferredSpawner.DeferredSpawnerTick();
		}
		catch (Exception ex8)
		{
			Log.Error(ex8.ToString());
		}
		try
		{
			lordManager.LordManagerTick();
		}
		catch (Exception ex9)
		{
			Log.Error(ex9.ToString());
		}
		try
		{
			passingShipManager.PassingShipManagerTick();
		}
		catch (Exception ex10)
		{
			Log.Error(ex10.ToString());
		}
		try
		{
			debugDrawer.DebugDrawerTick();
		}
		catch (Exception ex11)
		{
			Log.Error(ex11.ToString());
		}
		try
		{
			lordsStarter.VoluntarilyJoinableLordsStarterTick();
		}
		catch (Exception ex12)
		{
			Log.Error(ex12.ToString());
		}
		try
		{
			gameConditionManager.GameConditionManagerTick();
		}
		catch (Exception ex13)
		{
			Log.Error(ex13.ToString());
		}
		try
		{
			weatherManager.WeatherManagerTick();
		}
		catch (Exception ex14)
		{
			Log.Error(ex14.ToString());
		}
		try
		{
			resourceCounter.ResourceCounterTick();
		}
		catch (Exception ex15)
		{
			Log.Error(ex15.ToString());
		}
		try
		{
			weatherDecider.WeatherDeciderTick();
		}
		catch (Exception ex16)
		{
			Log.Error(ex16.ToString());
		}
		try
		{
			fireWatcher.FireWatcherTick();
		}
		catch (Exception ex17)
		{
			Log.Error(ex17.ToString());
		}
		if (ModsConfig.OdysseyActive)
		{
			try
			{
				waterBodyTracker?.Tick();
			}
			catch (Exception ex18)
			{
				Log.Error(ex18.ToString());
			}
		}
		try
		{
			flecks.FleckManagerTick();
		}
		catch (Exception ex19)
		{
			Log.Error(ex19.ToString());
		}
		try
		{
			effecterMaintainer.EffecterMaintainerTick();
		}
		catch (Exception ex20)
		{
			Log.Error(ex20.ToString());
		}
		MapComponentUtility.MapComponentTick(this);
		try
		{
			foreach (TileMutatorDef mutator in TileInfo.Mutators)
			{
				mutator.Worker?.Tick(this);
			}
		}
		catch (Exception ex21)
		{
			Log.Error(ex21.ToString());
		}
	}

	public void MapUpdate()
	{
		if (Disposed)
		{
			return;
		}
		bool drawingMap = WorldRendererUtility.DrawingMap;
		skyManager.SkyManagerUpdate();
		powerNetManager.UpdatePowerNetsAndConnections_First();
		regionGrid.UpdateClean();
		regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
		glowGrid.GlowGridUpdate_First();
		lordManager.LordManagerUpdate();
		postTickVisuals.ProcessPostTickVisuals();
		if (drawingMap && Find.CurrentMap == this)
		{
			if (AlwaysRedrawShadows)
			{
				mapDrawer.WholeMapChanged(MapMeshFlagDefOf.Things);
			}
			GlobalRendererUtility.UpdateGlobalShadersParams();
			PlantFallColors.SetFallShaderGlobals(this);
			waterInfo.SetTextures();
			avoidGrid.DebugDrawOnMap();
			BreachingGridDebug.DebugDrawAllOnMap(this);
			mapDrawer.MapMeshDrawerUpdate_First();
			powerNetGrid.DrawDebugPowerNetGrid();
			DoorsDebugDrawer.DrawDebug();
			mapDrawer.DrawMapMesh();
			dynamicDrawManager.DrawDynamicThings();
			gameConditionManager.GameConditionManagerDraw(this);
			MapEdgeClipDrawer.DrawClippers(this);
			designationManager.DrawDesignations();
			overlayDrawer.DrawAllOverlays();
			temporaryThingDrawer.Draw();
			flecks.FleckManagerDraw();
		}
		try
		{
			areaManager.AreaManagerUpdate();
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		weatherManager.WeatherManagerUpdate();
		try
		{
			flecks.FleckManagerUpdate();
		}
		catch (Exception ex2)
		{
			Log.Error(ex2.ToString());
		}
		MapComponentUtility.MapComponentUpdate(this);
	}

	public T GetComponent<T>() where T : MapComponent
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

	public MapComponent GetComponent(Type type)
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

	public void MapOnGUI()
	{
		DevGUISketches();
		DevRoadPaths();
		pathFinder.OnGUI();
	}

	private static void DevRoadPaths()
	{
		if (!DebugViewSettings.drawRoadPaths)
		{
			return;
		}
		for (int i = 0; i < GenStep_Roads.paths.Count; i++)
		{
			foreach (IntVec3 item in GenStep_Roads.paths[i])
			{
				Vector2 vector = item.ToVector3Shifted().MapToUIPosition();
				DevGUI.DrawRect(new Rect(vector.x, vector.y, 5f, 5f), (i % 2 == 0) ? Color.yellow : Color.blue);
			}
		}
	}

	private void DevGUISketches()
	{
		if ((!DebugViewSettings.drawMapGraphs && !DebugViewSettings.drawMapRooms) || layoutStructureSketches.NullOrEmpty())
		{
			return;
		}
		foreach (LayoutStructureSketch layoutStructureSketch in layoutStructureSketches)
		{
			DebugGUILayoutStructure(layoutStructureSketch);
		}
	}

	private void DebugGUILayoutStructure(LayoutStructureSketch layoutStructureSketch)
	{
		DevDrawOutline(layoutStructureSketch.structureLayout.container, Color.yellow);
		Vector2 pos = (layoutStructureSketch.structureLayout.container.Min - IntVec3.South).ToVector3().MapToUIPosition();
		DevDrawLabel(layoutStructureSketch.layoutDef.defName, pos);
		if (DebugViewSettings.drawMapGraphs && layoutStructureSketch.structureLayout?.neighbours != null)
		{
			foreach (KeyValuePair<Vector2, List<Vector2>> connection in layoutStructureSketch.structureLayout.neighbours.connections)
			{
				foreach (Vector2 item in connection.Value)
				{
					Vector2 vector = layoutStructureSketch.center.ToVector2();
					Vector2 vector2 = vector + connection.Key;
					Vector2 vector3 = vector + item;
					Vector2 start = new Vector3(vector2.x, 0f, vector2.y).MapToUIPosition();
					Vector2 end = new Vector3(vector3.x, 0f, vector3.y).MapToUIPosition();
					DevGUI.DrawLine(start, end, Color.green, 2f);
				}
			}
		}
		if (!DebugViewSettings.drawMapRooms || layoutStructureSketch.structureLayout?.Rooms == null)
		{
			return;
		}
		foreach (LayoutRoom room in layoutStructureSketch.structureLayout.Rooms)
		{
			string name = "NA";
			if (!room.defs.NullOrEmpty())
			{
				name = room.defs.Select((LayoutRoomDef x) => x.defName).ToCommaList();
			}
			DevDrawLabel(name, room.rects[0].CenterVector3.MapToUIPosition());
			foreach (CellRect rect in room.rects)
			{
				DevDrawOutline(rect, Color.blue);
			}
		}
	}

	private static void DevDrawLabel(string name, Vector2 pos)
	{
		float widthCached = name.GetWidthCached();
		DevGUI.Label(new Rect(pos.x - widthCached / 2f, pos.y, widthCached, 20f), name);
	}

	private static void DevDrawOutline(CellRect r, Color color)
	{
		IntVec3 min = r.Min;
		IntVec3 intVec = r.Max + new IntVec3(1, 0, 1);
		IntVec3 a = new IntVec3(min.x, 0, min.z);
		IntVec3 intVec2 = new IntVec3(intVec.x, 0, min.z);
		IntVec3 intVec3 = new IntVec3(min.x, 0, intVec.z);
		IntVec3 b = new IntVec3(intVec.x, 0, intVec.z);
		DevDrawLine(a, intVec2, color);
		DevDrawLine(a, intVec3, color);
		DevDrawLine(intVec3, b, color);
		DevDrawLine(intVec2, b, color);
	}

	private static void DevDrawLine(IntVec3 a, IntVec3 b, Color color)
	{
		Vector2 start = a.ToVector3().MapToUIPosition();
		Vector2 end = b.ToVector3().MapToUIPosition();
		DevGUI.DrawLine(start, end, color, 2f);
	}

	public string GetUniqueLoadID()
	{
		return "Map_" + uniqueID;
	}

	public override string ToString()
	{
		string text = "Map-" + uniqueID;
		if (IsPlayerHome)
		{
			text += "-PlayerHome";
		}
		return text;
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return spawnedThings;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder));
		List<PassingShip> passingShips = passingShipManager.passingShips;
		for (int i = 0; i < passingShips.Count; i++)
		{
			if (passingShips[i] is IThingHolder item)
			{
				outChildren.Add(item);
			}
		}
		for (int j = 0; j < components.Count; j++)
		{
			if (components[j] is IThingHolder item2)
			{
				outChildren.Add(item2);
			}
		}
	}

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		foreach (MapComponent component in components)
		{
			if (component is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		if (regionAndRoomUpdater != null)
		{
			regionAndRoomUpdater.Enabled = false;
		}
		pathFinder?.Dispose();
		lordManager?.Dispose();
		fogGrid?.Dispose();
		snowGrid?.Dispose();
		glowGrid?.Dispose();
		sandGrid?.Dispose();
		avoidGrid?.Dispose();
		listerBuildings?.Dispose();
		listerThings?.Clear();
		regionDirtyer?.SetAllDirty();
		regionGrid?.Dispose();
		pathing?.Dispose();
		mapDrawer?.Dispose();
		Resources.UnloadUnusedAssets();
		MapGenerator.ClearDebugMode();
	}
}
