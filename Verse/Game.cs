using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.Profile;

namespace Verse;

public class Game : IExposable, IDisposable
{
	private GameInitData initData;

	private Gravship gravshipInt;

	public sbyte currentMapIndex = -1;

	private GameInfo info = new GameInfo();

	public List<GameComponent> components = new List<GameComponent>();

	private GameRules rules = new GameRules();

	private Scenario scenarioInt;

	private World worldInt;

	private List<Map> maps = new List<Map>();

	public PlaySettings playSettings = new PlaySettings();

	public StoryWatcher storyWatcher = new StoryWatcher();

	public LetterStack letterStack = new LetterStack();

	public ResearchManager researchManager = new ResearchManager();

	public AnalysisManager analysisManager = new AnalysisManager();

	public GameEnder gameEnder = new GameEnder();

	public Storyteller storyteller = new Storyteller();

	public History history = new History();

	public TaleManager taleManager = new TaleManager();

	public PlayLog playLog = new PlayLog();

	public BattleLog battleLog = new BattleLog();

	public OutfitDatabase outfitDatabase = new OutfitDatabase();

	public DrugPolicyDatabase drugPolicyDatabase = new DrugPolicyDatabase();

	public ReadingPolicyDatabase readingPolicyDatabase = new ReadingPolicyDatabase();

	public FoodRestrictionDatabase foodRestrictionDatabase = new FoodRestrictionDatabase();

	public TickManager tickManager = new TickManager();

	public Tutor tutor = new Tutor();

	public Autosaver autosaver = new Autosaver();

	public DateNotifier dateNotifier = new DateNotifier();

	public SignalManager signalManager = new SignalManager();

	public UniqueIDsManager uniqueIDsManager = new UniqueIDsManager();

	public QuestManager questManager = new QuestManager();

	public TransportShipManager transportShipManager = new TransportShipManager();

	public StudyManager studyManager = new StudyManager();

	public CustomXenogermDatabase customXenogermDatabase = new CustomXenogermDatabase();

	public CustomXenotypeDatabase customXenotypeDatabase = new CustomXenotypeDatabase();

	public RelationshipRecords relationshipRecords = new RelationshipRecords();

	public HiddenItemsManager hiddenItemsManager = new HiddenItemsManager();

	public EntityCodex entityCodex = new EntityCodex();

	private static readonly List<Map> tmpPlayerHomeMaps = new List<Map>();

	public Scenario Scenario
	{
		get
		{
			return scenarioInt;
		}
		set
		{
			scenarioInt = value;
		}
	}

	public World World
	{
		get
		{
			return worldInt;
		}
		set
		{
			if (worldInt != value)
			{
				worldInt = value;
			}
		}
	}

	public Map CurrentMap
	{
		get
		{
			if (currentMapIndex < 0)
			{
				return null;
			}
			return maps[currentMapIndex];
		}
		set
		{
			int num;
			if (value == null)
			{
				num = -1;
			}
			else
			{
				num = maps.IndexOf(value);
				if (num < 0)
				{
					Log.Error("Could not set current map because it does not exist.");
					return;
				}
			}
			if (currentMapIndex != num)
			{
				currentMapIndex = (sbyte)num;
				Find.MapUI.Notify_SwitchedMap();
				AmbientSoundManager.Notify_SwitchedMap();
			}
		}
	}

	public Map AnyPlayerHomeMap
	{
		get
		{
			if (Faction.OfPlayerSilentFail == null)
			{
				return null;
			}
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome)
				{
					return maps[i];
				}
			}
			if (ModsConfig.OdysseyActive)
			{
				for (int j = 0; j < maps.Count; j++)
				{
					if (GravshipUtility.PlayerHasGravEngine(maps[j]))
					{
						return maps[j];
					}
				}
			}
			return null;
		}
	}

	public bool PlayerHasControl
	{
		get
		{
			if (ScreenFader.IsFading())
			{
				return false;
			}
			if (WorldComponent_GravshipController.CutsceneInProgress && !Find.CameraDriver.config.gravshipFreeCam)
			{
				return false;
			}
			return true;
		}
	}

	public IReadOnlyList<Map> PlayerHomeMaps
	{
		get
		{
			if (Faction.OfPlayerSilentFail == null)
			{
				return null;
			}
			tmpPlayerHomeMaps.Clear();
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
				{
					tmpPlayerHomeMaps.Add(map);
				}
			}
			return tmpPlayerHomeMaps;
		}
	}

	public Map RandomPlayerHomeMap
	{
		get
		{
			if (Faction.OfPlayerSilentFail == null)
			{
				return null;
			}
			tmpPlayerHomeMaps.Clear();
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
				{
					tmpPlayerHomeMaps.Add(map);
				}
			}
			if (tmpPlayerHomeMaps.Any())
			{
				Map result = tmpPlayerHomeMaps.RandomElement();
				tmpPlayerHomeMaps.Clear();
				return result;
			}
			return null;
		}
	}

	public Map RandomRootSurfacePlayerHomeMap
	{
		get
		{
			if (Faction.OfPlayerSilentFail == null)
			{
				return null;
			}
			tmpPlayerHomeMaps.Clear();
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome && map.Tile.Layer.IsRootSurface)
				{
					tmpPlayerHomeMaps.Add(map);
				}
			}
			if (tmpPlayerHomeMaps.Any())
			{
				Map result = tmpPlayerHomeMaps.RandomElement();
				tmpPlayerHomeMaps.Clear();
				return result;
			}
			return null;
		}
	}

	public Map RandomSurfacePlayerHomeMap
	{
		get
		{
			if (Faction.OfPlayerSilentFail == null)
			{
				return null;
			}
			tmpPlayerHomeMaps.Clear();
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome && map.Tile.LayerDef.SurfaceTiles)
				{
					tmpPlayerHomeMaps.Add(map);
				}
			}
			if (tmpPlayerHomeMaps.Any())
			{
				Map result = tmpPlayerHomeMaps.RandomElement();
				tmpPlayerHomeMaps.Clear();
				return result;
			}
			return null;
		}
	}

	public List<Map> Maps => maps;

	public GameInitData InitData
	{
		get
		{
			return initData;
		}
		set
		{
			initData = value;
		}
	}

	public GameInfo Info => info;

	public GameRules Rules => rules;

	public Gravship Gravship
	{
		get
		{
			return gravshipInt;
		}
		set
		{
			gravshipInt = value;
		}
	}

	public bool IsPlayerTile(PlanetTile tile)
	{
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (map.Tile == tile && map.IsPlayerHome)
			{
				return true;
			}
		}
		return false;
	}

	public Game()
	{
		FillComponents();
	}

	public void AddMap(Map map)
	{
		if (map == null)
		{
			Log.Error("Tried to add null map.");
			return;
		}
		if (maps.Contains(map))
		{
			Log.Error("Tried to add map but it's already here.");
			return;
		}
		if (maps.Count > 127)
		{
			Log.Error("Can't add map. Reached maps count limit (" + sbyte.MaxValue + ").");
			return;
		}
		maps.Add(map);
		Find.ColonistBar.MarkColonistsDirty();
	}

	public Map FindMap(MapParent mapParent)
	{
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].info.parent == mapParent)
			{
				return maps[i];
			}
		}
		return null;
	}

	public Map FindMap(PlanetTile tile)
	{
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].Tile == tile)
			{
				return maps[i];
			}
		}
		return null;
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			Log.Error("You must use special LoadData method to load Game.");
			return;
		}
		Scribe_Values.Look<sbyte>(ref currentMapIndex, "currentMapIndex", -1);
		ExposeSmallComponents();
		Scribe_Deep.Look(ref worldInt, "world");
		Scribe_Collections.Look(ref maps, "maps", LookMode.Deep);
		Find.CameraDriver.Expose();
	}

	private void ExposeSmallComponents()
	{
		Scribe_Deep.Look(ref info, "info");
		Scribe_Deep.Look(ref rules, "rules");
		Scribe_Deep.Look(ref scenarioInt, "scenario");
		Scribe_Deep.Look(ref tickManager, "tickManager");
		Scribe_Deep.Look(ref playSettings, "playSettings");
		Scribe_Deep.Look(ref storyWatcher, "storyWatcher");
		Scribe_Deep.Look(ref gameEnder, "gameEnder");
		Scribe_Deep.Look(ref letterStack, "letterStack");
		Scribe_Deep.Look(ref researchManager, "researchManager");
		Scribe_Deep.Look(ref analysisManager, "analysisManager");
		if (Scribe.mode == LoadSaveMode.LoadingVars && analysisManager == null)
		{
			analysisManager = new AnalysisManager();
		}
		Scribe_Deep.Look(ref storyteller, "storyteller");
		Scribe_Deep.Look(ref history, "history");
		Scribe_Deep.Look(ref taleManager, "taleManager");
		Scribe_Deep.Look(ref playLog, "playLog");
		Scribe_Deep.Look(ref battleLog, "battleLog");
		Scribe_Deep.Look(ref outfitDatabase, "outfitDatabase");
		Scribe_Deep.Look(ref drugPolicyDatabase, "drugPolicyDatabase");
		Scribe_Deep.Look(ref foodRestrictionDatabase, "foodRestrictionDatabase");
		Scribe_Deep.Look(ref readingPolicyDatabase, "readingPolicyDatabase");
		Scribe_Deep.Look(ref tutor, "tutor");
		Scribe_Deep.Look(ref dateNotifier, "dateNotifier");
		Scribe_Deep.Look(ref uniqueIDsManager, "uniqueIDsManager");
		Scribe_Deep.Look(ref questManager, "questManager");
		Scribe_Deep.Look(ref transportShipManager, "transportShipManager");
		Scribe_Deep.Look(ref studyManager, "studyManager");
		Scribe_Deep.Look(ref customXenogermDatabase, "customXenogermDatabase");
		Scribe_Deep.Look(ref customXenotypeDatabase, "customXenotypeDatabase");
		Scribe_Deep.Look(ref relationshipRecords, "relationshipRecords");
		Scribe_Deep.Look(ref hiddenItemsManager, "hiddenItemsManager");
		Scribe_Deep.Look(ref entityCodex, "entityCodex");
		Scribe_Collections.Look(ref components, "components", LookMode.Deep, this);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			FillComponents();
			if (rules == null)
			{
				Log.Warning("Save game was missing rules. Replacing with a blank GameRules.");
				rules = new GameRules();
			}
			if (relationshipRecords == null)
			{
				relationshipRecords = new RelationshipRecords();
			}
			if (readingPolicyDatabase == null)
			{
				readingPolicyDatabase = new ReadingPolicyDatabase();
			}
			if (hiddenItemsManager == null)
			{
				hiddenItemsManager = new HiddenItemsManager();
			}
			if (entityCodex == null)
			{
				entityCodex = new EntityCodex();
			}
		}
		BackCompatibility.PostExposeData(this);
	}

	private void FillComponents()
	{
		components.RemoveAll((GameComponent component) => component == null);
		foreach (Type item2 in typeof(GameComponent).AllSubclassesNonAbstract())
		{
			if (GetComponent(item2) == null)
			{
				try
				{
					GameComponent item = (GameComponent)Activator.CreateInstance(item2, this);
					components.Add(item);
				}
				catch (Exception ex)
				{
					Log.Error("Could not instantiate a GameComponent of type " + item2?.ToString() + ": " + ex);
				}
			}
		}
	}

	public void InitNewGame()
	{
		string text = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageIdPlayerFacing + ((!mod.ModMetaData.VersionCompatible) ? " (incompatible version)" : "")).ToLineList("  - ");
		Log.Message("Initializing new game with mods:\n" + text);
		if (maps.Any())
		{
			Log.Error("Called InitNewGame() but there already is a map. There should be 0 maps...");
			return;
		}
		if (initData == null)
		{
			Log.Error("Called InitNewGame() but init data is null. Create it first.");
			return;
		}
		ClearCaches();
		MemoryUtility.UnloadUnusedUnityAssets();
		try
		{
			Current.ProgramState = ProgramState.MapInitializing;
			IntVec3 intVec = new IntVec3(initData.mapSize, 1, initData.mapSize);
			Settlement settlement = null;
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			for (int num = 0; num < settlements.Count; num++)
			{
				if (settlements[num].Faction == Faction.OfPlayer)
				{
					settlement = settlements[num];
					break;
				}
			}
			if (settlement == null)
			{
				Log.Error("Could not generate starting map because there is no any player faction base.");
			}
			tickManager.gameStartAbsTick = GenTicks.ConfiguredTicksAbsAtGameStart;
			info.startingTile = initData.startingTile;
			info.startingAndOptionalPawns = initData.startingAndOptionalPawns;
			Map currentMap = MapGenerator.GenerateMap(intVec, settlement, initData.mapGeneratorDef ?? settlement.MapGeneratorDef, settlement.ExtraGenStepDefs);
			worldInt.info.initialMapSize = intVec;
			if (initData.permadeath)
			{
				info.permadeathMode = true;
				info.permadeathModeUniqueName = PermadeathModeUtility.GeneratePermadeathSaveName();
			}
			PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
			FinalizeInit();
			Current.Game.CurrentMap = currentMap;
			Find.CameraDriver.JumpToCurrentMapLoc(MapGenerator.PlayerStartSpot);
			Find.CameraDriver.ResetSize();
			if (Prefs.PauseOnLoad && initData.startedFromEntry)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					tickManager.DoSingleTick();
					tickManager.CurTimeSpeed = TimeSpeed.Paused;
				});
			}
			Find.Scenario.PostGameStart();
			history.FinalizeInit();
			ResearchUtility.ApplyPlayerStartingResearch();
			GameComponentUtility.StartedNewGame();
			initData = null;
		}
		finally
		{
		}
	}

	public void LoadGame()
	{
		if (maps.Any())
		{
			Log.Error("Called LoadGame() but there already is a map. There should be 0 maps...");
			return;
		}
		ClearCaches();
		MemoryUtility.UnloadUnusedUnityAssets();
		BackCompatibility.PreLoadSavegame(ScribeMetaHeaderUtility.loadedGameVersion);
		Current.ProgramState = ProgramState.MapInitializing;
		ExposeSmallComponents();
		LongEventHandler.SetCurrentEventText("LoadingWorld".Translate());
		if (Scribe.EnterNode("world"))
		{
			try
			{
				World = new World();
				World.ExposeData();
				Scribe.loader.crossRefs.RegisterForCrossRefResolve(World);
			}
			finally
			{
				Scribe.ExitNode();
			}
			DeepProfiler.Start("World.FinalizeInit");
			World.FinalizeInit(fromLoad: true);
			DeepProfiler.End();
			LongEventHandler.SetCurrentEventText("LoadingMap".Translate());
			Scribe_Collections.Look(ref maps, "maps", LookMode.Deep);
			if (maps.RemoveAll((Map x) => x == null) != 0)
			{
				Log.Warning("Some maps were null after loading.");
			}
			int value = -1;
			Scribe_Values.Look(ref value, "currentMapIndex", -1);
			if (value < 0 && maps.Any())
			{
				Log.Error("Current map is null after loading but there are maps available. Setting current map to [0].");
				value = 0;
			}
			if (value >= maps.Count)
			{
				Log.Error("Current map index out of bounds after loading.");
				value = ((!maps.Any()) ? (-1) : 0);
			}
			currentMapIndex = sbyte.MinValue;
			CurrentMap = ((value >= 0) ? maps[value] : null);
			LongEventHandler.SetCurrentEventText("InitializingGame".Translate());
			Find.CameraDriver.Expose();
			DeepProfiler.Start("Scribe.loader.FinalizeLoading");
			Scribe.loader.FinalizeLoading();
			DeepProfiler.End();
			LongEventHandler.SetCurrentEventText("SpawningAllThings".Translate());
			DeepProfiler.Start("maps.FinalizeLoading");
			for (int num = 0; num < maps.Count; num++)
			{
				try
				{
					maps[num].FinalizeLoading();
				}
				catch (Exception ex)
				{
					Log.Error("Error in Map.FinalizeLoading(): " + ex);
				}
				try
				{
					maps[num].Parent?.FinalizeLoading();
				}
				catch (Exception ex2)
				{
					Log.Error("Error in MapParent.FinalizeLoading(): " + ex2);
				}
			}
			DeepProfiler.End();
			DeepProfiler.Start("Game.FinalizeInit");
			FinalizeInit();
			DeepProfiler.End();
			if (Prefs.PauseOnLoad)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					Find.TickManager.DoSingleTick();
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				});
			}
			GameComponentUtility.LoadedGame();
			BackCompatibility.PostLoadSavegame(ScribeMetaHeaderUtility.loadedGameVersion);
		}
		else
		{
			Log.Error("Could not find world XML node.");
		}
	}

	public void UpdateEntry()
	{
		GameComponentUtility.GameComponentUpdate();
	}

	public void UpdatePlay()
	{
		try
		{
			Find.LetterStack.OpenAutomaticLetters();
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		tickManager.TickManagerUpdate();
		letterStack.LetterStackUpdate();
		World.WorldUpdate();
		for (int i = 0; i < maps.Count; i++)
		{
			maps[i].MapUpdate();
		}
		Info.GameInfoUpdate();
		GameComponentUtility.GameComponentUpdate();
		signalManager.SignalManagerUpdate();
		GlobalTextureAtlasManager.GlobalTextureAtlasManagerUpdate();
	}

	public T GetComponent<T>() where T : GameComponent
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

	public GameComponent GetComponent(Type type)
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

	public void FinalizeInit()
	{
		LogSimple.FlushToFileAndOpen();
		researchManager.ReapplyAllMods();
		MessagesRepeatAvoider.Reset();
		GameComponentUtility.FinalizeInit();
		Current.ProgramState = ProgramState.Playing;
		Current.Game.World.ideoManager.Notify_GameStarted();
		RecipeDefGenerator.ResetRecipeIngredientsForDifficulty();
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			DebugSettings.devPalette = Prefs.StartDevPaletteOn;
			Find.UIRoot.debugWindowOpener.TryOpenOrClosePalette();
		});
	}

	public void DeinitAndRemoveMap(Map map, bool notifyPlayer)
	{
		if (map == null)
		{
			Log.Error("Tried to remove null map.");
			return;
		}
		if (!maps.Contains(map))
		{
			Log.Error("Tried to remove map " + map?.ToString() + " but it's not here.");
			return;
		}
		if (map.Parent != null)
		{
			map.Parent.Notify_MyMapAboutToBeRemoved();
		}
		Map currentMap = CurrentMap;
		MapDeiniter.Deinit(map, notifyPlayer);
		maps.Remove(map);
		if (currentMap != null)
		{
			sbyte b = (sbyte)maps.IndexOf(currentMap);
			if (b < 0)
			{
				if (maps.Any())
				{
					CurrentMap = maps[0];
				}
				else
				{
					CurrentMap = null;
				}
				Find.World.renderer.wantedMode = WorldRenderMode.Planet;
			}
			else
			{
				currentMapIndex = b;
			}
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
		MapComponentUtility.MapRemoved(map);
		Find.Scenario.MapRemoved(map);
		if (map.Parent != null)
		{
			map.Parent.Notify_MyMapRemoved(map);
		}
		foreach (PocketMapParent item in Find.World.pocketMaps.ToList())
		{
			if (item.sourceMap == map && item.Map.generatorDef.pocketMapProperties.destroyOnParentMapAbandoned)
			{
				PocketMapUtility.DestroyPocketMap(item.Map);
			}
		}
		map.Dispose();
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Game debug data:");
		stringBuilder.AppendLine("initData:");
		if (initData == null)
		{
			stringBuilder.AppendLine("   null");
		}
		else
		{
			stringBuilder.AppendLine(initData.ToString());
		}
		stringBuilder.AppendLine("Scenario:");
		if (scenarioInt == null)
		{
			stringBuilder.AppendLine("   null");
		}
		else
		{
			stringBuilder.AppendLine("   " + scenarioInt);
		}
		stringBuilder.AppendLine("World:");
		if (worldInt == null)
		{
			stringBuilder.AppendLine("   null");
		}
		else
		{
			stringBuilder.AppendLine("   name: " + worldInt.info.name);
		}
		stringBuilder.AppendLine("Maps count: " + maps.Count);
		for (int i = 0; i < maps.Count; i++)
		{
			stringBuilder.AppendLine("   Map " + maps[i].Index + ":");
			stringBuilder.AppendLine("      tile: " + maps[i].TileInfo);
		}
		stringBuilder.AppendLine("Game components:");
		for (int j = 0; j < components.Count; j++)
		{
			components[j].AppendDebugString(stringBuilder);
		}
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		for (int i = 0; i < maps.Count; i++)
		{
			maps[i].Dispose();
		}
		worldInt?.Dispose();
		SteadyEnvironmentEffects.Reset();
	}

	public static void ClearCaches()
	{
		Find.ClearCache();
		ChildcareUtility.ClearCache();
		SlaveRebellionUtility.ClearCache();
		Alert_NeedMeditationSpot.ClearCache();
		BuildCopyCommandUtility.ClearCache();
		MechanitorUtility.ClearCache();
		SocialCardUtility.ClearCaches();
		foreach (StatDef item in DefDatabase<StatDef>.AllDefsListForReading)
		{
			item.Worker.TryClearCache();
		}
	}
}
