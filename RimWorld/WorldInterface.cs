using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class WorldInterface
	{
		public WorldSelector selector = new WorldSelector();

		public WorldTargeter targeter = new WorldTargeter();

		public WorldInspectPane inspectPane = new WorldInspectPane();

		public WorldGlobalControls globalControls = new WorldGlobalControls();

		public WorldRoutePlanner routePlanner = new WorldRoutePlanner();

		public bool everReset;

		public int SelectedTile
		{
			get
			{
				return selector.selectedTile;
			}
			set
			{
				selector.selectedTile = value;
			}
		}

		public void Reset()
		{
			everReset = true;
			inspectPane.Reset();
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (Find.CurrentMap != null)
				{
					SelectedTile = Find.CurrentMap.Tile;
				}
				else
				{
					SelectedTile = -1;
				}
			}
			else if (Find.GameInitData != null)
			{
				if (Find.GameInitData.startingTile >= 0 && Find.World != null && !Find.WorldGrid.InBounds(Find.GameInitData.startingTile))
				{
					Log.Error("Map world tile was out of bounds.");
					Find.GameInitData.startingTile = -1;
				}
				SelectedTile = Find.GameInitData.startingTile;
				inspectPane.OpenTabType = typeof(WITab_Terrain);
			}
			else
			{
				SelectedTile = -1;
			}
			if (SelectedTile >= 0)
			{
				Find.WorldCameraDriver.JumpTo(SelectedTile);
			}
			else
			{
				Find.WorldCameraDriver.JumpTo(Find.WorldGrid.viewCenter);
			}
			Find.WorldCameraDriver.ResetAltitude();
		}

		public void WorldInterfaceUpdate()
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				targeter.TargeterUpdate();
				WorldSelectionDrawer.DrawSelectionOverlays();
				Find.WorldDebugDrawer.WorldDebugDrawerUpdate();
			}
			else
			{
				targeter.StopTargeting();
			}
			routePlanner.WorldRoutePlannerUpdate();
		}

		public void WorldInterfaceOnGUI()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			CheckOpenOrCloseInspectPane();
			if (worldRenderedNow)
			{
				ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
				ExpandableWorldObjectsUtility.ExpandableWorldObjectsOnGUI();
				WorldSelectionDrawer.SelectionOverlaysOnGUI();
				routePlanner.WorldRoutePlannerOnGUI();
				if (!screenshotMode.FiltersCurrentEvent && Current.ProgramState == ProgramState.Playing)
				{
					Find.ColonistBar.ColonistBarOnGUI();
				}
				selector.dragBox.DragBoxOnGUI();
				targeter.TargeterOnGUI();
				if (!screenshotMode.FiltersCurrentEvent)
				{
					globalControls.WorldGlobalControlsOnGUI();
				}
				Find.WorldDebugDrawer.WorldDebugDrawerOnGUI();
			}
		}

		public void HandleLowPriorityInput()
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				targeter.ProcessInputEvents();
				selector.WorldSelectorOnGUI();
			}
		}

		private void CheckOpenOrCloseInspectPane()
		{
			if (selector.AnyObjectOrTileSelected && WorldRendererUtility.WorldRenderedNow && (Current.ProgramState != ProgramState.Playing || Find.MainTabsRoot.OpenTab == null))
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
}
