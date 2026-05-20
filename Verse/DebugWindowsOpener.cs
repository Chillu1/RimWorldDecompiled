using System;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class DebugWindowsOpener
{
	private Action drawButtonsCached;

	private WidgetRow widgetRow = new WidgetRow();

	private float widgetRowFinalX;

	public DebugWindowsOpener()
	{
		drawButtonsCached = DrawButtons;
	}

	public void DevToolStarterOnGUI()
	{
		if (Prefs.DevMode)
		{
			Vector2 vector = new Vector2((float)UI.screenWidth * 0.5f - widgetRowFinalX * 0.5f, 3f);
			float height = 25f;
			Find.WindowStack.ImmediateWindow(1593759361, new Rect(vector.x, vector.y, widgetRowFinalX, height).Rounded(), WindowLayer.GameUI, drawButtonsCached, doBackground: false, absorbInputAroundWindow: false, 0f);
			if (KeyBindingDefOf.Dev_ToggleDebugLog.KeyDownEvent)
			{
				ToggleLogWindow();
				Event.current.Use();
			}
			if (KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent)
			{
				ToggleDebugActionsMenu();
				Event.current.Use();
			}
			if (KeyBindingDefOf.Dev_ToggleDebugLogMenu.KeyDownEvent)
			{
				ToggleDebugLogMenu();
				Event.current.Use();
			}
			if (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent)
			{
				ToggleDebugSettingsMenu();
				Event.current.Use();
			}
			if (KeyBindingDefOf.Dev_ToggleDevPalette.KeyDownEvent && Current.ProgramState == ProgramState.Playing)
			{
				DebugSettings.devPalette = !DebugSettings.devPalette;
				TryOpenOrClosePalette();
				Event.current.Use();
			}
			if (KeyBindingDefOf.Dev_ToggleDebugInspector.KeyDownEvent)
			{
				ToggleDebugInspector();
				Event.current.Use();
			}
			if (Current.ProgramState == ProgramState.Playing && KeyBindingDefOf.Dev_ToggleGodMode.KeyDownEvent)
			{
				ToggleGodMode();
				Event.current.Use();
			}
		}
	}

	private void DrawButtons()
	{
		widgetRow.Init(0f, 0f);
		if (widgetRow.ButtonIcon(TexButton.ToggleLog, "Open the debug log."))
		{
			ToggleLogWindow();
		}
		if (widgetRow.ButtonIcon(TexButton.ToggleTweak, "Open tweakvalues menu.\n\nThis lets you change internal values."))
		{
			ToggleTweakValuesMenu();
		}
		if (widgetRow.ButtonIcon(TexButton.OpenDebugActionsMenu, "Open debug actions menu.\n\nThis lets you spawn items and force various events."))
		{
			ToggleDebugActionsMenu();
		}
		if (widgetRow.ButtonIcon(TexButton.OpenInspectSettings, "Open the view settings.\n\nThis lets you see special debug visuals."))
		{
			ToggleDebugSettingsMenu();
		}
		if (widgetRow.ButtonIcon(TexButton.OpenDebugActionsMenu, "Open debug logging menu."))
		{
			ToggleDebugLogMenu();
		}
		if (widgetRow.ButtonIcon(TexButton.OpenInspector, "Open the inspector.\n\nThis lets you inspect what's happening in the game, down to individual variables."))
		{
			ToggleDebugInspector();
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			bool toggleable = DebugSettings.devPalette;
			widgetRow.ToggleableIcon(ref toggleable, TexButton.ToggleDevPalette, "Toggle the dev palette.\n\nAllows you to setup a palette of debug actions for ease of use.");
			if (toggleable != DebugSettings.devPalette)
			{
				DebugSettings.devPalette = toggleable;
				TryOpenOrClosePalette();
			}
			if (widgetRow.ButtonIcon(DebugSettings.godMode ? TexButton.GodModeEnabled : TexButton.GodModeDisabled, "Toggle god mode.\n\nWhen god mode is on, you can build stuff instantly, for free, and sell things that aren't yours."))
			{
				ToggleGodMode();
			}
		}
		widgetRowFinalX = widgetRow.FinalX;
	}

	private void ToggleLogWindow()
	{
		if (!Find.WindowStack.TryRemove(typeof(EditWindow_Log)))
		{
			Find.WindowStack.Add(new EditWindow_Log());
		}
	}

	private void ToggleDebugSettingsMenu()
	{
		Dialog_Debug dialog_Debug = Find.WindowStack.WindowOfType<Dialog_Debug>();
		if (dialog_Debug == null)
		{
			Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Settings));
		}
		else
		{
			dialog_Debug.SwitchTab(DebugTabMenuDefOf.Settings);
		}
	}

	private void ToggleDebugActionsMenu()
	{
		Dialog_Debug dialog_Debug = Find.WindowStack.WindowOfType<Dialog_Debug>();
		if (dialog_Debug == null)
		{
			Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Actions));
		}
		else
		{
			dialog_Debug.SwitchTab(DebugTabMenuDefOf.Actions);
		}
	}

	private void ToggleTweakValuesMenu()
	{
		if (!Find.WindowStack.TryRemove(typeof(EditWindow_TweakValues)))
		{
			Find.WindowStack.Add(new EditWindow_TweakValues());
		}
	}

	private void ToggleDebugLogMenu()
	{
		Dialog_Debug dialog_Debug = Find.WindowStack.WindowOfType<Dialog_Debug>();
		if (dialog_Debug == null)
		{
			Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Output));
		}
		else
		{
			dialog_Debug.SwitchTab(DebugTabMenuDefOf.Output);
		}
	}

	private void ToggleDebugInspector()
	{
		if (!Find.WindowStack.TryRemove(typeof(EditWindow_DebugInspector)))
		{
			Find.WindowStack.Add(new EditWindow_DebugInspector());
		}
	}

	private void ToggleGodMode()
	{
		DebugSettings.godMode = !DebugSettings.godMode;
		if (DebugSettings.godMode)
		{
			SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
		}
		else
		{
			SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
		}
	}

	public void TryOpenOrClosePalette()
	{
		if (DebugSettings.devPalette)
		{
			Find.WindowStack.Add(new Dialog_DevPalette());
		}
		else
		{
			Find.WindowStack.TryRemove(typeof(Dialog_DevPalette));
		}
	}
}
