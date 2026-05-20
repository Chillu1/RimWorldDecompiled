using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class WorldInterface
{
	public WorldSelector selector = new WorldSelector();

	public WorldTargeter targeter = new WorldTargeter();

	public WorldInspectPane inspectPane = new WorldInspectPane();

	public WorldGlobalControls globalControls = new WorldGlobalControls();

	public WorldRoutePlanner routePlanner = new WorldRoutePlanner();

	public TilePicker tilePicker = new TilePicker();

	public bool everReset;

	public PlanetTile SelectedTile
	{
		get
		{
			return selector.SelectedTile;
		}
		set
		{
			selector.SelectedTile = value;
		}
	}

	public void Reset()
	{
		everReset = true;
		inspectPane.Reset();
		if (Current.ProgramState == ProgramState.Playing)
		{
			SelectedTile = ((Find.CurrentMap != null) ? Find.CurrentMap.Tile : PlanetTile.Invalid);
		}
		else if (Find.GameInitData != null)
		{
			if (Find.GameInitData.startingTile.Valid && Find.World != null && !Find.WorldGrid.InBounds(Find.GameInitData.startingTile))
			{
				Log.Error("Map world tile was out of bounds.");
				Find.GameInitData.startingTile = PlanetTile.Invalid;
			}
			SelectedTile = Find.GameInitData.startingTile;
			inspectPane.OpenTabType = typeof(WITab_Terrain);
		}
		else
		{
			SelectedTile = PlanetTile.Invalid;
		}
		if (SelectedTile.Valid)
		{
			Find.WorldCameraDriver.JumpTo(SelectedTile);
		}
		else
		{
			Find.WorldCameraDriver.JumpTo(Find.WorldGrid.SurfaceViewCenter);
		}
		Find.WorldCameraDriver.ResetAltitude();
	}

	public void WorldInterfaceUpdate()
	{
		if (WorldRendererUtility.WorldSelected)
		{
			targeter.TargeterUpdate();
			WorldSelectionDrawer.DrawSelectionOverlays();
			if (tilePicker.Active)
			{
				tilePicker.TileSelectorUpdate();
			}
			Find.WorldDebugDrawer.WorldDebugDrawerUpdate();
		}
		else
		{
			targeter.StopTargeting();
			tilePicker.StopTargeting();
		}
		routePlanner.WorldRoutePlannerUpdate();
		WorldGizmoUtility.WorldUIUpdate();
	}

	public void WorldInterfaceOnGUI()
	{
		CheckOpenOrCloseInspectPane();
		if (WorldRendererUtility.WorldSelected && Find.WorldCamera.gameObject.activeInHierarchy)
		{
			ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
			ExpandableWorldObjectsUtility.ExpandableWorldObjectsOnGUI();
			if (ModsConfig.OdysseyActive)
			{
				ExpandableLandmarksUtility.ExpandableLandmarksOnGUI();
			}
			WorldSelectionDrawer.SelectionOverlaysOnGUI();
			routePlanner.WorldRoutePlannerOnGUI();
			if (!screenshotMode.FiltersCurrentEvent && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.ColonistBarOnGUI();
			}
			selector.dragBox.DragBoxOnGUI();
			targeter.TargeterOnGUI();
			if (tilePicker.Active)
			{
				tilePicker.TileSelectorOnGUI();
			}
			if (!screenshotMode.FiltersCurrentEvent)
			{
				globalControls.WorldGlobalControlsOnGUI();
			}
			WorldGizmoUtility.WorldUIOnGUI();
			Find.WorldDebugDrawer.WorldDebugDrawerOnGUI();
		}
	}

	public void HandleLowPriorityInput()
	{
		if (WorldRendererUtility.WorldSelected)
		{
			targeter.ProcessInputEvents();
			selector.WorldSelectorOnGUI();
		}
	}

	private void CheckOpenOrCloseInspectPane()
	{
		if (selector.AnyObjectOrTileSelected && WorldRendererUtility.WorldSelected && (Current.ProgramState != ProgramState.Playing || Find.MainTabsRoot.OpenTab == null))
		{
			if (!Find.WindowStack.IsOpen<WorldInspectPane>())
			{
				Find.WindowStack.Add(inspectPane);
			}
		}
		else if (Find.WindowStack.IsOpen<WorldInspectPane>())
		{
			Find.WindowStack.TryRemove(inspectPane, doCloseSound: false);
		}
	}
}
