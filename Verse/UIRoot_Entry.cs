using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public class UIRoot_Entry : UIRoot
	{
		private bool ShouldDoMainMenu
		{
			get
			{
				if (LongEventHandler.AnyEventNowOrWaiting)
				{
					return false;
				}
				for (int i = 0; i < Find.WindowStack.Count; i++)
				{
					if (windows[i].layer == WindowLayer.Dialog && !Find.WindowStack[i].IsDebug)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override void Init()
		{
			base.Init();
			UIMenuBackgroundManager.background = new UI_BackgroundMain();
			MainMenuDrawer.Init();
			QuickStarter.CheckQuickStart();
			VersionUpdateDialogMaker.CreateVersionUpdateDialogIfNecessary();
			if (!SteamManager.Initialized)
			{
				Dialog_MessageBox window = new Dialog_MessageBox((string)"SteamClientMissing".Translate(), "Quit".Translate(), delegate
				{
					Application.Quit();
				}, "Ignore".Translate());
				Find.WindowStack.Add(window);
			}
		}

		public override void UIRootOnGUI()
		{
			base.UIRootOnGUI();
			if (Find.World != null)
			{
				Find.World.UI.WorldInterfaceOnGUI();
			}
			DoMainMenu();
			if (Current.Game != null)
			{
				Find.Tutor.TutorOnGUI();
			}
			ReorderableWidget.ReorderableWidgetOnGUI_BeforeWindowStack();
			windows.WindowStackOnGUI();
			ReorderableWidget.ReorderableWidgetOnGUI_AfterWindowStack();
			Widgets.WidgetsOnGUI();
			if (Find.World != null)
			{
				Find.World.UI.HandleLowPriorityInput();
			}
		}

		public override void UIRootUpdate()
		{
			base.UIRootUpdate();
			if (Find.World != null)
			{
				Find.World.UI.WorldInterfaceUpdate();
			}
			if (Current.Game != null)
			{
				LessonAutoActivator.LessonAutoActivatorUpdate();
				Find.Tutor.TutorUpdate();
			}
		}

		private void DoMainMenu()
		{
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				UIMenuBackgroundManager.background.BackgroundOnGUI();
				if (ShouldDoMainMenu)
				{
					Current.Game = null;
					MainMenuDrawer.MainMenuOnGUI();
				}
			}
		}
	}
}
