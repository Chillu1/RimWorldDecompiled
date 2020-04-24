using RimWorld;
using UnityEngine;
using Verse.Noise;
using Verse.Sound;

namespace Verse
{
	public abstract class UIRoot
	{
		public WindowStack windows = new WindowStack();

		protected DebugWindowsOpener debugWindowOpener = new DebugWindowsOpener();

		public ScreenshotModeHandler screenshotMode = new ScreenshotModeHandler();

		private ShortcutKeys shortcutKeys = new ShortcutKeys();

		public FeedbackFloaters feedbackFloaters = new FeedbackFloaters();

		public virtual void Init()
		{
		}

		public virtual void UIRootOnGUI()
		{
			UnityGUIBugsFixer.OnGUI();
			Text.StartOfOnGUI();
			CheckOpenLogWindow();
			DelayedErrorWindowRequest.DelayedErrorWindowRequestOnGUI();
			DebugInputLogger.InputLogOnGUI();
			if (!screenshotMode.FiltersCurrentEvent)
			{
				debugWindowOpener.DevToolStarterOnGUI();
			}
			windows.HandleEventsHighPriority();
			screenshotMode.ScreenshotModesOnGUI();
			if (!screenshotMode.FiltersCurrentEvent)
			{
				TooltipHandler.DoTooltipGUI();
				feedbackFloaters.FeedbackOnGUI();
				DragSliderManager.DragSlidersOnGUI();
				Messages.MessagesDoGUI();
			}
			shortcutKeys.ShortcutKeysOnGUI();
			NoiseDebugUI.NoiseDebugOnGUI();
			Debug.developerConsoleVisible = false;
			if (Current.Game != null)
			{
				GameComponentUtility.GameComponentOnGUI();
			}
		}

		public virtual void UIRootUpdate()
		{
			ScreenshotTaker.Update();
			DragSliderManager.DragSlidersUpdate();
			windows.WindowsUpdate();
			MouseoverSounds.ResolveFrame();
			UIHighlighter.UIHighlighterUpdate();
			Messages.Update();
		}

		private void CheckOpenLogWindow()
		{
			if (EditWindow_Log.wantsToOpen && !Find.WindowStack.IsOpen(typeof(EditWindow_Log)))
			{
				Find.WindowStack.Add(new EditWindow_Log());
				EditWindow_Log.wantsToOpen = false;
			}
		}
	}
}
