using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class UIRoot_Play : UIRoot
	{
		public MapInterface mapUI = new MapInterface();

		public MainButtonsRoot mainButtonsRoot = new MainButtonsRoot();

		public AlertsReadout alerts = new AlertsReadout();

		public override void Init()
		{
			base.Init();
			Messages.Clear();
		}

		public override void UIRootOnGUI()
		{
			base.UIRootOnGUI();
			Find.GameInfo.GameInfoOnGUI();
			Find.World.UI.WorldInterfaceOnGUI();
			mapUI.MapInterfaceOnGUI_BeforeMainTabs();
			if (!screenshotMode.FiltersCurrentEvent)
			{
				mainButtonsRoot.MainButtonsOnGUI();
				alerts.AlertsReadoutOnGUI();
			}
			mapUI.MapInterfaceOnGUI_AfterMainTabs();
			if (!screenshotMode.FiltersCurrentEvent)
			{
				Find.Tutor.TutorOnGUI();
			}
			ReorderableWidget.ReorderableWidgetOnGUI_BeforeWindowStack();
			windows.WindowStackOnGUI();
			ReorderableWidget.ReorderableWidgetOnGUI_AfterWindowStack();
			Widgets.WidgetsOnGUI();
			mapUI.HandleMapClicks();
			if (Find.DesignatorManager.SelectedDesignator != null)
			{
				Find.DesignatorManager.SelectedDesignator.SelectedProcessInput(Event.current);
			}
			DebugTools.DebugToolsOnGUI();
			mainButtonsRoot.HandleLowPriorityShortcuts();
			Find.World.UI.HandleLowPriorityInput();
			mapUI.HandleLowPriorityInput();
			OpenMainMenuShortcut();
		}

		public override void UIRootUpdate()
		{
			base.UIRootUpdate();
			try
			{
				Find.World.UI.WorldInterfaceUpdate();
				mapUI.MapInterfaceUpdate();
				alerts.AlertsReadoutUpdate();
				LessonAutoActivator.LessonAutoActivatorUpdate();
				Find.Tutor.TutorUpdate();
			}
			catch (Exception ex)
			{
				Log.Error("Exception in UIRootUpdate: " + ex.ToString());
			}
		}

		private void OpenMainMenuShortcut()
		{
			if ((Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) || KeyBindingDefOf.Cancel.KeyDownEvent)
			{
				Event.current.Use();
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Menu);
			}
		}
	}
}
