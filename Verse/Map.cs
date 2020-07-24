using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public sealed class Map : IIncidentTarget, ILoadReferenceable, IThingHolder, IExposable
	{
		public MapFileCompressor compressor;

		private List<Thing> loadedFullThings;

		public int uniqueID = -1;

		public int generationTick;

		public MapInfo info = new MapInfo();

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

		public PhysicalInteractionReservationManager physicalInteractionReservationManager;

		public DesignationManager designationManager;

		public LordManager lordManager;

		public PassingShipManager passingShipManager;

		public HaulDestinationManager haulDestinationManager;

		public DebugCellDrawer debugDrawer;

		public GameConditionManager gameConditionManager;

		public WeatherManager weatherManager;

		public ZoneManager zoneManager;

		public ResourceCounter resourceCounter;

		public MapTemperature mapTemperature;

		public TemperatureCache temperatureCache;

		public AreaManager areaManager;

		public AttackTargetsCache attackTargetsCache;

		public AttackTargetReservationManager attackTargetReservationManager;

		public VoluntarilyJoinableLordsStarter lordsStarter;

		public ThingGrid thingGrid;

		public CoverGrid coverGrid;

		public EdificeGrid edificeGrid;

		public BlueprintGrid blueprintGrid;

		public FogGrid fogGrid;

		public RegionGrid regionGrid;

		public GlowGrid glowGrid;

		public TerrainGrid terrainGrid;

		public PathGrid pathGrid;

		public RoofGrid roofGrid;

		public FertilityGrid fertilityGrid;

		public SnowGrid snowGrid;

		public DeepResourceGrid deepResourceGrid;

		public ExitMapGrid exitMapGrid;

		public AvoidGrid avoidGrid;

		public LinkGrid linkGrid;

		public GlowFlooder glowFlooder;

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

		public ListerFilthInHomeArea listerFilthInHomeArea;

		public Reachability reachability;

		public ItemAvailability itemAvailability;

		public AutoBuildRoofAreaSetter autoBuildRoofAreaSetter;

		public RoofCollapseBufferResolver roofCollapseBufferResolver;

		public RoofCollapseBuffer roofCollapseBuffer;

		public WildAnimalSpawner wildAnimalSpawner;

		public WildPlantSpawner wildPlantSpawner;

		public SteadyEnvironmentEffects steadyEnvironmentEffects;

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

		public const string ThingSaveKey = "thing";

		[TweakValue("Graphics_Shadow", 0f, 100f)]
		private static bool AlwaysRedrawShadows;

		public int Index => Find.Maps.IndexOf(this);

		public IntVec3 Size => info.Size;

		public IntVec3 Center => new IntVec3(Size.x / 2, 0, Size.z / 2);

		public Faction ParentFaction => info.parent.Faction;

		public int Area => Size.x * Size.z;

		public IThingHolder ParentHolder => info.parent;

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
				if (info != null && info.parent.def.canBePlayerHome)
				{
					return info.parent.Faction == Faction.OfPlayer;
				}
				return false;
			}
		}

		public bool IsTempIncidentMap => info.parent.def.isTempIncidentMapOwner;

		public int Tile => info.Tile;

		public Tile TileInfo => Find.WorldGrid[Tile];

		public BiomeDef Biome => TileInfo.biome;

		public StoryState StoryState => storyState;

		public GameConditionManager GameConditionManager => gameConditionManager;

		public float PlayerWealthForStoryteller
		{
			get
			{
				if (IsPlayerHome)
				{
					return wealthWatcher.WealthItems + wealthWatcher.WealthBuildings * 0.5f + wealthWatcher.WealthPawns;
				}
				float num = 0f;
				foreach (Pawn item in mapPawns.PawnsInFaction(Faction.OfPlayer))
				{
					if (item.IsFreeColonist)
					{
						num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(item);
					}
					if (item.RaceProps.Animal)
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
			return info.parent.IncidentTargetTags();
		}

		public void ConstructComponents()
		{
			spawnedThings = new ThingOwner<Thing>(this);
			cellIndices = new CellIndices(this);
			listerThings = new ListerThings(ListerThingsUse.Global);
			listerBuildings = new ListerBuildings();
			mapPawns = new MapPawns(this);
			dynamicDrawManager = new DynamicDrawManager(this);
			mapDrawer = new MapDrawer(this);
			tooltipGiverList = new TooltipGiverList();
			pawnDestinationReservationManager = new PawnDestinationReservationManager();
			reservationManager = new ReservationManager(this);
			physicalInteractionReservationManager = new PhysicalInteractionReservationManager();
			designationManager = new DesignationManager(this);
			lordManager = new LordManager(this);
			debugDrawer = new DebugCellDrawer();
			passingShipManager = new PassingShipManager(this);
			haulDestinationManager = new HaulDestinationManager(this);
			gameConditionManager = new GameConditionManager(this);
			weatherManager = new WeatherManager(this);
			zoneManager = new ZoneManager(this);
			resourceCounter = new ResourceCounter(this);
			mapTemperature = new MapTemperature(this);
			temperatureCache = new TemperatureCache(this);
			areaManager = new AreaManager(this);
			attackTargetsCache = new AttackTargetsCache(this);
			attackTargetReservationManager = new AttackTargetReservationManager(this);
			lordsStarter = new VoluntarilyJoinableLordsStarter(this);
			thingGrid = new ThingGrid(this);
			coverGrid = new CoverGrid(this);
			edificeGrid = new EdificeGrid(this);
			blueprintGrid = new BlueprintGrid(this);
			fogGrid = new FogGrid(this);
			glowGrid = new GlowGrid(this);
			regionGrid = new RegionGrid(this);
			terrainGrid = new TerrainGrid(this);
			pathGrid = new PathGrid(this);
			roofGrid = new RoofGrid(this);
			fertilityGrid = new FertilityGrid(this);
			snowGrid = new SnowGrid(this);
			deepResourceGrid = new DeepResourceGrid(this);
			exitMapGrid = new ExitMapGrid(this);
			avoidGrid = new AvoidGrid(this);
			linkGrid = new LinkGrid(this);
			glowFlooder = new GlowFlooder(this);
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
			reachability = new Reachability(this);
			itemAvailability = new ItemAvailability(this);
			autoBuildRoofAreaSetter = new AutoBuildRoofAreaSetter(this);
			roofCollapseBufferResolver = new RoofCollapseBufferResolver(this);
			roofCollapseBuffer = new RoofCollapseBuffer();
			wildAnimalSpawner = new WildAnimalSpawner(this);
			wildPlantSpawner = new WildPlantSpawner(this);
			steadyEnvironmentEffects = new SteadyEnvironmentEffects(this);
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
			components.Clear();
			FillComponents();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref uniqueID, "uniqueID", -1);
			Scribe_Values.Look(ref generationTick, "generationTick", 0);
			Scribe_Deep.Look(ref info, "mapInfo");
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
									if (hashSet.Contains(allThing.ThingID))
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
							catch (Exception ex2)
							{
								Log.Error(string.Concat("Exception saving ", allThing, ": ", ex2));
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
				ExposeComponents();
				DeepProfiler.Start("Load compressed things");
				compressor.ExposeData();
				DeepProfiler.End();
				DeepProfiler.Start("Load non-compressed things");
				Scribe_Collections.Look(ref loadedFullThings, "things", LookMode.Deep);
				DeepProfiler.End();
			}
		}

		private void FillComponents()
		{
			components.RemoveAll((MapComponent component) => component == null);
			foreach (Type item2 in typeof(MapComponent).AllSubclassesNonAbstract())
			{
				if (GetComponent(item2) == null)
				{
					try
					{
						MapComponent item = (MapComponent)Activator.CreateInstance(item2, this);
						components.Add(item);
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat("Could not instantiate a MapComponent of type ", item2, ": ", ex));
					}
				}
			}
			roadInfo = GetComponent<RoadInfo>();
			waterInfo = GetComponent<WaterInfo>();
		}

		public void FinalizeLoading()
		{
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
			pathGrid.RecalculateAllPerceivedPathCosts();
			regionAndRoomUpdater.Enabled = true;
			regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			powerNetManager.UpdatePowerNetsAndConnections_First();
			temperatureCache.temperatureSaveLoad.ApplyLoadedDataToRegions();
			avoidGrid.Regenerate();
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
			listerFilthInHomeArea.RebuildAll();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				mapDrawer.RegenerateEverythingNow();
			});
			resourceCounter.UpdateResourceCounts();
			wealthWatcher.ForceRecount(allowDuringInit: true);
			MapComponentUtility.FinalizeInit(this);
		}

		private void ExposeComponents()
		{
			Scribe_Deep.Look(ref weatherManager, "weatherManager", this);
			Scribe_Deep.Look(ref reservationManager, "reservationManager", this);
			Scribe_Deep.Look(ref physicalInteractionReservationManager, "physicalInteractionReservationManager");
			Scribe_Deep.Look(ref designationManager, "designationManager", this);
			Scribe_Deep.Look(ref pawnDestinationReservationManager, "pawnDestinationReservationManager");
			Scribe_Deep.Look(ref lordManager, "lordManager", this);
			Scribe_Deep.Look(ref passingShipManager, "visitorManager", this);
			Scribe_Deep.Look(ref gameConditionManager, "gameConditionManager", this);
			Scribe_Deep.Look(ref fogGrid, "fogGrid", this);
			Scribe_Deep.Look(ref roofGrid, "roofGrid", this);
			Scribe_Deep.Look(ref terrainGrid, "terrainGrid", this);
			Scribe_Deep.Look(ref zoneManager, "zoneManager", this);
			Scribe_Deep.Look(ref temperatureCache, "temperatureCache", this);
			Scribe_Deep.Look(ref snowGrid, "snowGrid", this);
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
			Scribe_Deep.Look(ref wildPlantSpawner, "wildPlantSpawner", this);
			Scribe_Collections.Look(ref components, "components", LookMode.Deep, this);
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
				lordManager.LordManagerTick();
			}
			catch (Exception ex5)
			{
				Log.Error(ex5.ToString());
			}
			try
			{
				passingShipManager.PassingShipManagerTick();
			}
			catch (Exception ex6)
			{
				Log.Error(ex6.ToString());
			}
			try
			{
				debugDrawer.DebugDrawerTick();
			}
			catch (Exception ex7)
			{
				Log.Error(ex7.ToString());
			}
			try
			{
				lordsStarter.VoluntarilyJoinableLordsStarterTick();
			}
			catch (Exception ex8)
			{
				Log.Error(ex8.ToString());
			}
			try
			{
				gameConditionManager.GameConditionManagerTick();
			}
			catch (Exception ex9)
			{
				Log.Error(ex9.ToString());
			}
			try
			{
				weatherManager.WeatherManagerTick();
			}
			catch (Exception ex10)
			{
				Log.Error(ex10.ToString());
			}
			try
			{
				resourceCounter.ResourceCounterTick();
			}
			catch (Exception ex11)
			{
				Log.Error(ex11.ToString());
			}
			try
			{
				weatherDecider.WeatherDeciderTick();
			}
			catch (Exception ex12)
			{
				Log.Error(ex12.ToString());
			}
			try
			{
				fireWatcher.FireWatcherTick();
			}
			catch (Exception ex13)
			{
				Log.Error(ex13.ToString());
			}
			MapComponentUtility.MapComponentTick(this);
		}

		public void MapUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			skyManager.SkyManagerUpdate();
			powerNetManager.UpdatePowerNetsAndConnections_First();
			regionGrid.UpdateClean();
			regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
			glowGrid.GlowGridUpdate_First();
			lordManager.LordManagerUpdate();
			if (!worldRenderedNow && Find.CurrentMap == this)
			{
				if (AlwaysRedrawShadows)
				{
					mapDrawer.WholeMapChanged(MapMeshFlag.Things);
				}
				PlantFallColors.SetFallShaderGlobals(this);
				waterInfo.SetTextures();
				avoidGrid.DebugDrawOnMap();
				mapDrawer.MapMeshDrawerUpdate_First();
				powerNetGrid.DrawDebugPowerNetGrid();
				DoorsDebugDrawer.DrawDebug();
				mapDrawer.DrawMapMesh();
				dynamicDrawManager.DrawDynamicThings();
				gameConditionManager.GameConditionManagerDraw(this);
				MapEdgeClipDrawer.DrawClippers(this);
				designationManager.DrawDesignations();
				overlayDrawer.DrawAllOverlays();
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
			MapComponentUtility.MapComponentUpdate(this);
		}

		public T GetComponent<T>() where T : MapComponent
		{
			for (int i = 0; i < components.Count; i++)
			{
				T val = components[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		public MapComponent GetComponent(Type type)
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (type.IsAssignableFrom(components[i].GetType()))
				{
					return components[i];
				}
			}
			return null;
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
				IThingHolder thingHolder = passingShips[i] as IThingHolder;
				if (thingHolder != null)
				{
					outChildren.Add(thingHolder);
				}
			}
			for (int j = 0; j < components.Count; j++)
			{
				IThingHolder thingHolder2 = components[j] as IThingHolder;
				if (thingHolder2 != null)
				{
					outChildren.Add(thingHolder2);
				}
			}
		}
	}
}
