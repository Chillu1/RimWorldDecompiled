using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Verse.Sound;

namespace Verse;

public static class Find
{
	private static GameComponent_Anomaly cachedAnomaly;

	public static Root Root => Current.Root;

	public static SoundRoot SoundRoot => Current.Root.soundRoot;

	public static UIRoot UIRoot
	{
		get
		{
			if (!(Current.Root != null))
			{
				return null;
			}
			return Current.Root.uiRoot;
		}
	}

	public static MusicManagerEntry MusicManagerEntry => ((Root_Entry)Current.Root).musicManagerEntry;

	public static MusicManagerPlay MusicManagerPlay => ((Root_Play)Current.Root).musicManagerPlay;

	public static LanguageWorker ActiveLanguageWorker => LanguageDatabase.activeLanguage.Worker;

	public static Camera Camera => Current.Camera;

	public static CameraDriver CameraDriver => Current.CameraDriver;

	public static ColorCorrectionCurves CameraColor => Current.ColorCorrectionCurves;

	public static Camera PawnCacheCamera => PawnCacheCameraManager.PawnCacheCamera;

	public static PawnCacheRenderer PawnCacheRenderer => PawnCacheCameraManager.PawnCacheRenderer;

	public static Camera WorldCamera => WorldCameraManager.WorldCamera;

	public static WorldCameraDriver WorldCameraDriver => WorldCameraManager.WorldCameraDriver;

	public static WindowStack WindowStack => UIRoot?.windows;

	public static ScreenshotModeHandler ScreenshotModeHandler => UIRoot?.screenshotMode;

	public static MainButtonsRoot MainButtonsRoot => ((UIRoot_Play)UIRoot).mainButtonsRoot;

	public static MainTabsRoot MainTabsRoot => MainButtonsRoot.tabs;

	public static MapInterface MapUI => ((UIRoot_Play)UIRoot).mapUI;

	public static AlertsReadout Alerts => ((UIRoot_Play)UIRoot).alerts;

	public static Selector Selector => MapUI.selector;

	public static Targeter Targeter => MapUI.targeter;

	public static ColonistBar ColonistBar => MapUI.colonistBar;

	public static DesignatorManager DesignatorManager => MapUI.designatorManager;

	public static ReverseDesignatorDatabase ReverseDesignatorDatabase => MapUI.reverseDesignatorDatabase;

	public static GameInitData GameInitData => Current.Game?.InitData;

	public static GameInfo GameInfo => Current.Game.Info;

	public static Scenario Scenario
	{
		get
		{
			if (Current.Game != null && Current.Game.Scenario != null)
			{
				return Current.Game.Scenario;
			}
			if (ScenarioMaker.GeneratingScenario != null)
			{
				return ScenarioMaker.GeneratingScenario;
			}
			if (UIRoot != null)
			{
				Page_ScenarioEditor page_ScenarioEditor = WindowStack.WindowOfType<Page_ScenarioEditor>();
				if (page_ScenarioEditor != null)
				{
					return page_ScenarioEditor.EditingScenario;
				}
			}
			return null;
		}
	}

	public static World World
	{
		get
		{
			if (Current.Game == null || Current.Game.World == null)
			{
				return Current.CreatingWorld;
			}
			return Current.Game.World;
		}
	}

	public static List<Map> Maps => Current.Game?.Maps;

	public static Map CurrentMap => Current.Game?.CurrentMap;

	public static Map AnyPlayerHomeMap => Current.Game?.AnyPlayerHomeMap;

	public static Map RandomPlayerHomeMap => Current.Game?.RandomPlayerHomeMap;

	public static Map RandomRootSurfacePlayerHomeMap => Current.Game?.RandomRootSurfacePlayerHomeMap;

	public static Map RandomSurfacePlayerHomeMap => Current.Game?.RandomRootSurfacePlayerHomeMap;

	public static StoryWatcher StoryWatcher => Current.Game.storyWatcher;

	public static ResearchManager ResearchManager => Current.Game.researchManager;

	public static AnalysisManager AnalysisManager => Current.Game.analysisManager;

	public static Storyteller Storyteller => Current.Game?.storyteller;

	public static GameEnder GameEnder => Current.Game.gameEnder;

	public static LetterStack LetterStack => Current.Game.letterStack;

	public static Archive Archive => History?.archive;

	public static PlaySettings PlaySettings => Current.Game.playSettings;

	public static History History => Current.Game?.history;

	public static TaleManager TaleManager => Current.Game.taleManager;

	public static PlayLog PlayLog => Current.Game.playLog;

	public static BattleLog BattleLog => Current.Game.battleLog;

	public static TickManager TickManager => Current.Game.tickManager;

	public static Tutor Tutor => Current.Game?.tutor;

	public static TutorialState TutorialState => Current.Game.tutor.tutorialState;

	public static ActiveLessonHandler ActiveLesson => Current.Game?.tutor.activeLesson;

	public static Autosaver Autosaver => Current.Game.autosaver;

	public static DateNotifier DateNotifier => Current.Game.dateNotifier;

	public static SignalManager SignalManager => Current.Game.signalManager;

	public static UniqueIDsManager UniqueIDsManager => Current.Game?.uniqueIDsManager;

	public static QuestManager QuestManager => Current.Game.questManager;

	public static RelationshipRecords RelationshipRecords => Current.Game.relationshipRecords;

	public static TransportShipManager TransportShipManager => Current.Game.transportShipManager;

	public static GameComponent_Bossgroup BossgroupManager => Current.Game.GetComponent<GameComponent_Bossgroup>();

	public static GameComponent_Anomaly Anomaly => cachedAnomaly ?? (cachedAnomaly = Current.Game.GetComponent<GameComponent_Anomaly>());

	public static StudyManager StudyManager => Current.Game.studyManager;

	public static CustomXenogermDatabase CustomXenogermDatabase => Current.Game.customXenogermDatabase;

	public static GameComponent_PsychicRitualManager PsychicRitualManager => Current.Game.GetComponent<GameComponent_PsychicRitualManager>();

	public static GameComponent_PawnDuplicator PawnDuplicator => Current.Game.GetComponent<GameComponent_PawnDuplicator>();

	public static HiddenItemsManager HiddenItemsManager => Current.Game.hiddenItemsManager;

	public static EntityCodex EntityCodex => Current.Game.entityCodex;

	public static Gravship CurrentGravship => Current.Game.Gravship;

	public static FactionManager FactionManager => World.factionManager;

	public static IdeoManager IdeoManager => World.ideoManager;

	public static WorldPawns WorldPawns => World.worldPawns;

	public static WorldObjectsHolder WorldObjects => World.worldObjects;

	public static WorldGrid WorldGrid => World.grid;

	public static WorldDebugDrawer WorldDebugDrawer => World.debugDrawer;

	public static WorldPathGrid WorldPathGrid => World.pathGrid;

	public static WorldDynamicDrawManager WorldDynamicDrawManager => World.dynamicDrawManager;

	public static WorldPathPool WorldPathPool => World.pathPool;

	public static WorldReachability WorldReachability => World.reachability;

	public static WorldFeatures WorldFeatures => World.features;

	public static WorldComponent_GravshipController GravshipController => World.GetComponent<WorldComponent_GravshipController>();

	public static WorldInterface WorldInterface => World.UI;

	public static WorldSelector WorldSelector => WorldInterface.selector;

	public static WorldTargeter WorldTargeter => WorldInterface.targeter;

	public static WorldRoutePlanner WorldRoutePlanner => WorldInterface.routePlanner;

	public static TilePicker TilePicker => WorldInterface.tilePicker;

	public static HistoryEventsManager HistoryEventsManager => History.historyEventsManager;

	public static GoodwillSituationManager GoodwillSituationManager => FactionManager.goodwillSituationManager;

	public static void ClearCache()
	{
		cachedAnomaly = null;
	}
}
