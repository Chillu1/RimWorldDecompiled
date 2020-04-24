using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class MapInterface
	{
		public ThingOverlays thingOverlays = new ThingOverlays();

		public Selector selector = new Selector();

		public Targeter targeter = new Targeter();

		public DesignatorManager designatorManager = new DesignatorManager();

		public ReverseDesignatorDatabase reverseDesignatorDatabase = new ReverseDesignatorDatabase();

		private MouseoverReadout mouseoverReadout = new MouseoverReadout();

		public GlobalControls globalControls = new GlobalControls();

		protected ResourceReadout resourceReadout = new ResourceReadout();

		public ColonistBar colonistBar = new ColonistBar();

		public void MapInterfaceOnGUI_BeforeMainTabs()
		{
			if (Find.CurrentMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
				thingOverlays.ThingOverlaysOnGUI();
				MapComponentUtility.MapComponentOnGUI(Find.CurrentMap);
				BeautyDrawer.BeautyDrawerOnGUI();
				if (!screenshotMode.FiltersCurrentEvent)
				{
					colonistBar.ColonistBarOnGUI();
				}
				selector.dragBox.DragBoxOnGUI();
				designatorManager.DesignationManagerOnGUI();
				targeter.TargeterOnGUI();
				Find.CurrentMap.tooltipGiverList.DispenseAllThingTooltips();
				if (DebugViewSettings.drawFoodSearchFromMouse)
				{
					FoodUtility.DebugFoodSearchFromMouse_OnGUI();
				}
				if (DebugViewSettings.drawAttackTargetScores)
				{
					AttackTargetFinder.DebugDrawAttackTargetScores_OnGUI();
				}
				if (!screenshotMode.FiltersCurrentEvent)
				{
					mouseoverReadout.MouseoverReadoutOnGUI();
					globalControls.GlobalControlsOnGUI();
					resourceReadout.ResourceReadoutOnGUI();
				}
			}
			else
			{
				targeter.StopTargeting();
			}
		}

		public void MapInterfaceOnGUI_AfterMainTabs()
		{
			if (Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow && !Find.UIRoot.screenshotMode.FiltersCurrentEvent)
			{
				EnvironmentStatsDrawer.EnvironmentStatsOnGUI();
				Find.CurrentMap.debugDrawer.DebugDrawerOnGUI();
			}
		}

		public void HandleMapClicks()
		{
			if (Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				designatorManager.ProcessInputEvents();
				targeter.ProcessInputEvents();
			}
		}

		public void HandleLowPriorityInput()
		{
			if (Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				selector.SelectorOnGUI();
				Find.CurrentMap.lordManager.LordManagerOnGUI();
			}
		}

		public void MapInterfaceUpdate()
		{
			if (Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				targeter.TargeterUpdate();
				SelectionDrawer.DrawSelectionOverlays();
				EnvironmentStatsDrawer.DrawRoomOverlays();
				designatorManager.DesignatorManagerUpdate();
				Find.CurrentMap.roofGrid.RoofGridUpdate();
				Find.CurrentMap.fertilityGrid.FertilityGridUpdate();
				Find.CurrentMap.terrainGrid.TerrainGridUpdate();
				Find.CurrentMap.exitMapGrid.ExitMapGridUpdate();
				Find.CurrentMap.deepResourceGrid.DeepResourceGridUpdate();
				if (DebugViewSettings.drawPawnDebug)
				{
					Find.CurrentMap.pawnDestinationReservationManager.DebugDrawDestinations();
					Find.CurrentMap.reservationManager.DebugDrawReservations();
				}
				if (DebugViewSettings.drawDestReservations)
				{
					Find.CurrentMap.pawnDestinationReservationManager.DebugDrawReservations();
				}
				if (DebugViewSettings.drawFoodSearchFromMouse)
				{
					FoodUtility.DebugFoodSearchFromMouse_Update();
				}
				if (DebugViewSettings.drawPreyInfo)
				{
					FoodUtility.DebugDrawPredatorFoodSource();
				}
				if (DebugViewSettings.drawAttackTargetScores)
				{
					AttackTargetFinder.DebugDrawAttackTargetScores_Update();
				}
				MiscDebugDrawer.DebugDrawInteractionCells();
				Find.CurrentMap.debugDrawer.DebugDrawerUpdate();
				Find.CurrentMap.regionGrid.DebugDraw();
				InfestationCellFinder.DebugDraw();
				StealAIDebugDrawer.DebugDraw();
				if (DebugViewSettings.drawRiverDebug)
				{
					Find.CurrentMap.waterInfo.DebugDrawRiver();
				}
				BuildingsDamageSectionLayerUtility.DebugDraw();
			}
		}

		public void Notify_SwitchedMap()
		{
			designatorManager.Deselect();
			reverseDesignatorDatabase.Reinit();
			selector.ClearSelection();
			selector.dragBox.active = false;
			targeter.StopTargeting();
			MainButtonDef openTab = Find.MainTabsRoot.OpenTab;
			List<MainButtonDef> allDefsListForReading = DefDatabase<MainButtonDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				allDefsListForReading[i].Notify_SwitchedMap();
			}
			if (openTab != null && openTab != MainButtonDefOf.Inspect)
			{
				Find.MainTabsRoot.SetCurrentTab(openTab, playSound: false);
			}
			if (Find.CurrentMap != null)
			{
				RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
				Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
			}
		}
	}
}
