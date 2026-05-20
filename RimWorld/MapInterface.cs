using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

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
		if (WorldRendererUtility.DrawingMap)
		{
			ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
			thingOverlays.ThingOverlaysOnGUI();
			Find.CurrentMap.MapOnGUI();
			MapComponentUtility.MapComponentOnGUI(Find.CurrentMap);
			BeautyDrawer.BeautyDrawerOnGUI();
			if (!screenshotMode.FiltersCurrentEvent)
			{
				colonistBar.ColonistBarOnGUI();
			}
			selector.dragBox.DragBoxOnGUI();
			designatorManager.DesignationManagerOnGUI();
			targeter.TargeterOnGUI();
			selector.SelectorOnGUI_BeforeMainTabs();
			Find.CurrentMap.tooltipGiverList.DispenseAllThingTooltips();
			Find.CurrentMap.flecks.FleckManagerOnGUI();
			if (DebugViewSettings.drawFoodSearchFromMouse)
			{
				FoodUtility.DebugFoodSearchFromMouse_OnGUI();
			}
			if (DebugViewSettings.drawNonCombatantTimer)
			{
				AttackTargetFinder.DebugDrawNonCombatantTimer_OnGUI();
			}
			if (DebugViewSettings.drawAttackTargetScores)
			{
				AttackTargetFinder.DebugDrawAttackTargetScores_OnGUI();
			}
			if (ModsConfig.OdysseyActive && GravshipUtility.ShowConnectedSubstructure)
			{
				using (new ProfilerBlock("DrawSubstructureCountOnGUI()"))
				{
					SubstructureGrid.DrawSubstructureCountOnGUI();
				}
			}
			if (!screenshotMode.FiltersCurrentEvent)
			{
				mouseoverReadout.MouseoverReadoutOnGUI();
				globalControls.GlobalControlsOnGUI();
				resourceReadout.ResourceReadoutOnGUI();
				MapGizmoUtility.MapUIOnGUI();
			}
		}
		else
		{
			targeter.StopTargeting();
		}
	}

	public void MapInterfaceOnGUI_AfterMainTabs()
	{
		if (Find.CurrentMap != null && WorldRendererUtility.DrawingMap && !Find.UIRoot.screenshotMode.FiltersCurrentEvent)
		{
			EnvironmentStatsDrawer.EnvironmentStatsOnGUI();
			Find.CurrentMap.deepResourceGrid.DeepResourcesOnGUI();
			Find.CurrentMap.debugDrawer.DebugDrawerOnGUI();
		}
	}

	public void HandleMapClicks()
	{
		if (Find.CurrentMap != null && WorldRendererUtility.DrawingMap)
		{
			designatorManager.ProcessInputEvents();
			targeter.ProcessInputEvents();
		}
	}

	public void HandleLowPriorityInput()
	{
		if (Find.CurrentMap != null && WorldRendererUtility.DrawingMap)
		{
			selector.SelectorOnGUI();
			Find.CurrentMap.lordManager.LordManagerOnGUI();
		}
	}

	public void MapInterfaceUpdate()
	{
		if (Find.CurrentMap == null || !WorldRendererUtility.DrawingMap)
		{
			return;
		}
		targeter.TargeterUpdate();
		SelectionDrawer.DrawSelectionOverlays();
		EnvironmentStatsDrawer.DrawRoomOverlays();
		designatorManager.DesignatorManagerUpdate();
		selector.gotoController.Draw();
		Find.CurrentMap.roofGrid.RoofGridUpdate();
		Find.CurrentMap.fertilityGrid.FertilityGridUpdate();
		if (ModsConfig.BiotechActive)
		{
			Find.CurrentMap.pollutionGrid.PollutionGridUpdate();
		}
		using (new ProfilerBlock("MapComponentOnDraw()"))
		{
			MapComponentUtility.MapComponentOnDraw(Find.CurrentMap);
		}
		Find.CurrentMap.pathFinder.OnDraw();
		if (ModsConfig.OdysseyActive)
		{
			using (new ProfilerBlock("SubstructureGridUpdate()"))
			{
				Find.CurrentMap.substructureGrid?.DrawSubstructureGrid();
			}
			if (GravshipUtility.ShowConnectedSubstructure)
			{
				using (new ProfilerBlock("SubstructureGridUpdate()"))
				{
					SubstructureGrid.DrawSubstructureFootprint();
				}
			}
		}
		Find.CurrentMap.terrainGrid.TerrainGridUpdate();
		Find.CurrentMap.exitMapGrid.ExitMapGridUpdate();
		Find.CurrentMap.deepResourceGrid.DeepResourceGridUpdate();
		Find.CurrentMap.mapTemperature.TemperatureUpdate();
		MapGizmoUtility.MapUIUpdate();
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
		if (DebugViewSettings.drawFOVSymmetry)
		{
			GenSight.DebugDrawFOVSymmetry_Update();
		}
		MiscDebugDrawer.DebugDrawInteractionCells();
		Find.CurrentMap.debugDrawer.DebugDrawerUpdate();
		Find.CurrentMap.regionGrid.DebugDraw();
		InfestationCellFinder.DebugDraw();
		LargeBuildingCellFinder.DebugDraw();
		StealAIDebugDrawer.DebugDraw();
		MapGenerator.DebugDraw();
		Find.CurrentMap.waterInfo.DebugDrawRiver();
		BuildingsDamageSectionLayerUtility.DebugDraw();
		Find.CurrentMap.waterBodyTracker?.DebugDraw();
	}

	public void Notify_SwitchedMap()
	{
		designatorManager.Deselect();
		reverseDesignatorDatabase.Reinit();
		selector.ClearSelection();
		selector.dragBox.active = false;
		selector.gotoController.Deactivate();
		targeter.StopTargeting();
		Designator_AreaAllowed.ClearSelectedArea();
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
