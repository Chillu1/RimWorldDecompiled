using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class DebugWindowsOpener
	{
		private Action drawButtonsCached;

		private WidgetRow widgetRow = new WidgetRow();

		public DebugWindowsOpener()
		{
			drawButtonsCached = DrawButtons;
		}

		public void DevToolStarterOnGUI()
		{
			if (Prefs.DevMode)
			{
				Vector2 vector = new Vector2((float)UI.screenWidth * 0.5f, 3f);
				int num = 6;
				if (Current.ProgramState == ProgramState.Playing)
				{
					num += 2;
				}
				float num2 = 25f;
				if (Current.ProgramState == ProgramState.Playing && DebugSettings.godMode)
				{
					num2 += 15f;
				}
				Find.WindowStack.ImmediateWindow(1593759361, new Rect(vector.x, vector.y, (float)num * 28f - 4f + 1f, num2).Rounded(), WindowLayer.GameUI, drawButtonsCached, doBackground: false, absorbInputAroundWindow: false, 0f);
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
			if (widgetRow.ButtonIcon(TexButton.OpenInspectSettings, "Open the view settings.\n\nThis lets you see special debug visuals."))
			{
				ToggleDebugSettingsMenu();
			}
			if (widgetRow.ButtonIcon(TexButton.OpenDebugActionsMenu, "Open debug actions menu.\n\nThis lets you spawn items and force various events."))
			{
				ToggleDebugActionsMenu();
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
				if (widgetRow.ButtonIcon(TexButton.ToggleGodMode, "Toggle god mode.\n\nWhen god mode is on, you can build stuff instantly, for free, and sell things that aren't yours."))
				{
					ToggleGodMode();
				}
				if (DebugSettings.godMode)
				{
					Text.Font = GameFont.Tiny;
					Widgets.Label(new Rect(0f, 25f, 200f, 100f), "God mode");
				}
				bool toggleable = Prefs.PauseOnError;
				widgetRow.ToggleableIcon(ref toggleable, TexButton.TogglePauseOnError, "Pause the game when an error is logged.");
				Prefs.PauseOnError = toggleable;
			}
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
			if (!Find.WindowStack.TryRemove(typeof(Dialog_DebugSettingsMenu)))
			{
				Find.WindowStack.Add(new Dialog_DebugSettingsMenu());
			}
		}

		private void ToggleDebugActionsMenu()
		{
			if (!Find.WindowStack.TryRemove(typeof(Dialog_DebugActionsMenu)))
			{
				Find.WindowStack.Add(new Dialog_DebugActionsMenu());
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
			if (!Find.WindowStack.TryRemove(typeof(Dialog_DebugOutputMenu)))
			{
				Find.WindowStack.Add(new Dialog_DebugOutputMenu());
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
		}
	}
}
