using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse.Noise;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

public abstract class UIRoot
{
	public WindowStack windows = new WindowStack();

	public DebugWindowsOpener debugWindowOpener = new DebugWindowsOpener();

	public ScreenshotModeHandler screenshotMode = new ScreenshotModeHandler();

	private ShortcutKeys shortcutKeys = new ShortcutKeys();

	public FeedbackFloaters feedbackFloaters = new FeedbackFloaters();

	public bool HideMotes
	{
		get
		{
			if (WorldComponent_GravshipController.CutsceneInProgress)
			{
				return true;
			}
			return false;
		}
	}

	public virtual void Init()
	{
	}

	public virtual void UIRootOnGUI()
	{
		DebugInputLogger.InputLogOnGUI();
		UnityGUIBugsFixer.OnGUI();
		SteamDeck.OnGUI();
		SteamDeck.RootOnGUI();
		OriginalEventUtility.RecordOriginalEvent(Event.current);
		Text.StartOfOnGUI();
		CheckOpenLogWindow();
		DelayedErrorWindowRequest.DelayedErrorWindowRequestOnGUI();
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
			CellInspectorDrawer.OnGUI();
		}
		Find.World?.WorldOnGUI();
		OriginalEventUtility.Reset();
	}

	public virtual void UIRootUpdate()
	{
		ScreenshotTaker.Update();
		DragSliderManager.DragSlidersUpdate();
		windows.WindowsUpdate();
		MouseoverSounds.ResolveFrame();
		UIHighlighter.UIHighlighterUpdate();
		Messages.Update();
		CellInspectorDrawer.Update();
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
