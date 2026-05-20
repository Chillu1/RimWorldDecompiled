using System.Collections.Generic;
using RimWorld;
using Steamworks;
using UnityEngine;

namespace Verse.Steam;

public static class SteamDeck
{
	private static bool isSteamDeck;

	private static bool keyboardShowing;

	private static int lastFocusedTextFieldID;

	private static int lastFocusedTextFieldCursorIndex;

	private static bool nextLeftMouseUpEventIsRightMouseUpEvent;

	private static bool unfocusCurrentTextField;

	private static Callback<FloatingGamepadTextInputDismissed_t> keyboardDismissedCallback;

	private static Rect? currentScrollView;

	private static Vector2 currentScrollViewStartMousePos;

	private static bool scrollMouseTraveledEnoughDist;

	private static int consumeAllMouseUpEventsOnFrame = -1;

	private static Rect? scrollViewWithVelocity;

	private static Vector2 scrollViewVelocity;

	private static List<(float, Vector2)> scrollVelocityRecords = new List<(float, Vector2)>();

	public static bool IsSteamDeck
	{
		get
		{
			if (!isSteamDeck)
			{
				return DebugSettings.simulateUsingSteamDeck;
			}
			return true;
		}
	}

	public static bool IsSteamDeckInNonKeyboardMode
	{
		get
		{
			if (IsSteamDeck)
			{
				return !Prefs.SteamDeckKeyboardMode;
			}
			return false;
		}
	}

	public static bool KeyboardShowing => keyboardShowing;

	public static void Init()
	{
		isSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
		if (isSteamDeck)
		{
			SteamInput.Init(bExplicitlyCallRunFrame: false);
			keyboardDismissedCallback = Callback<FloatingGamepadTextInputDismissed_t>.Create(KeyboardDismissedCallback);
		}
	}

	public static void ShowOnScreenKeyboard(string initialText, Rect textFieldRect, bool multiline)
	{
		if (isSteamDeck && !keyboardShowing)
		{
			Rect rect = GUIUtility.GUIToScreenRect(textFieldRect);
			if (SteamUtils.ShowFloatingGamepadTextInput(multiline ? EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeMultipleLines : EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height))
			{
				keyboardShowing = true;
			}
		}
	}

	public static void HideOnScreenKeyboard()
	{
		if (isSteamDeck && keyboardShowing)
		{
			SteamUtils.DismissFloatingGamepadTextInput();
		}
	}

	public static void ShowConfigPage()
	{
		if (isSteamDeck)
		{
			SteamInput.ShowBindingPanel(new InputHandle_t(0uL));
		}
	}

	public static void Shutdown()
	{
		if (isSteamDeck)
		{
			SteamInput.Shutdown();
		}
	}

	public static void Update()
	{
		if (!isSteamDeck)
		{
			return;
		}
		if (IsSteamDeckInNonKeyboardMode)
		{
			TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			if (textEditor != null && (textEditor.controlID != lastFocusedTextFieldID || textEditor.cursorIndex != lastFocusedTextFieldCursorIndex) && textEditor.position != default(Rect) && !UnityGUIBugsFixer.IsLeftMouseButtonPressed())
			{
				lastFocusedTextFieldID = textEditor.controlID;
				lastFocusedTextFieldCursorIndex = textEditor.cursorIndex;
				ShowOnScreenKeyboard(textEditor.text, textEditor.position, textEditor.multiline);
			}
		}
		else
		{
			keyboardShowing = false;
		}
	}

	public static void OnGUI()
	{
		if (!keyboardShowing)
		{
			return;
		}
		TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		string text = textEditor.text;
		if (!text.NullOrEmpty())
		{
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperCenter;
			float height = Text.CalcHeight(text, 464f) + 36f;
			float num = Mathf.Min(Text.CalcSize(text).x, 464f) + 36f;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Rect rect = new Rect((float)UI.screenWidth / 2f - num / 2f, 30f, num, height);
			Find.WindowStack.ImmediateWindow(84906312, rect, WindowLayer.Super, delegate
			{
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.UpperCenter;
				Rect rect2 = rect.AtZero().ContractedBy(18f);
				rect2.height += 10f;
				Widgets.Label(rect2, text);
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			});
		}
	}

	public static void Vibrate()
	{
		if (isSteamDeck)
		{
			SteamInput.TriggerVibration(new InputHandle_t(0uL), 10000, 10000);
		}
	}

	public static void RootOnGUI()
	{
		if (!IsSteamDeck)
		{
			return;
		}
		SimulateRightClickIfHoldingMiddleButton();
		if (unfocusCurrentTextField && Event.current.type == EventType.Repaint)
		{
			unfocusCurrentTextField = false;
			UI.UnfocusCurrentTextField();
			TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			if (textEditor != null)
			{
				lastFocusedTextFieldID = textEditor.controlID;
				lastFocusedTextFieldCursorIndex = textEditor.cursorIndex;
			}
		}
	}

	public static void WindowOnGUI()
	{
		if (IsSteamDeck)
		{
			SimulateRightClickIfHoldingMiddleButton();
		}
	}

	private static void SimulateRightClickIfHoldingMiddleButton()
	{
		if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && Event.current.button == 0 && (Input.GetMouseButton(2) || Input.GetMouseButtonUp(2) || (nextLeftMouseUpEventIsRightMouseUpEvent && Event.current.type == EventType.MouseUp)))
		{
			Event.current.button = 1;
			if (Event.current.type == EventType.MouseDown)
			{
				nextLeftMouseUpEventIsRightMouseUpEvent = true;
			}
			else if (Event.current.type == EventType.MouseUp)
			{
				nextLeftMouseUpEventIsRightMouseUpEvent = false;
			}
		}
	}

	private static void KeyboardDismissedCallback(FloatingGamepadTextInputDismissed_t data)
	{
		keyboardShowing = false;
		unfocusCurrentTextField = true;
	}

	public static void ShowSteamDeckGameControlsIfNotKnown()
	{
		if (IsSteamDeckInNonKeyboardMode && Find.TickManager.TicksGame > 120 && !Find.TickManager.Paused && !PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.SteamDeckControlsGame))
		{
			Dialog_MessageBox dialog_MessageBox = new Dialog_MessageBox(ConceptDefOf.SteamDeckControlsGame.HelpTextAdjusted);
			dialog_MessageBox.image = ContentFinder<Texture2D>.Get("UI/Misc/SteamDeck2");
			Find.WindowStack.Add(dialog_MessageBox);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.SteamDeckControlsGame, KnowledgeAmount.Total);
		}
	}

	public static void ShowSteamDeckMainMenuControlsIfNotKnown()
	{
		if (IsSteamDeckInNonKeyboardMode && !PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.SteamDeckControlsMainMenu))
		{
			Dialog_MessageBox dialog_MessageBox = new Dialog_MessageBox(ConceptDefOf.SteamDeckControlsMainMenu.HelpTextAdjusted);
			dialog_MessageBox.image = ContentFinder<Texture2D>.Get("UI/Misc/SteamDeck1");
			Find.WindowStack.Add(dialog_MessageBox);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.SteamDeckControlsMainMenu, KnowledgeAmount.Total);
		}
	}

	public static void HandleTouchScreenScrollViewScroll(Rect outRect, ref Vector2 scrollPosition)
	{
		if (!IsSteamDeck || DragAndDropWidget.Dragging || Widgets.Painting || ReorderableWidget.Dragging)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0) && Mouse.IsOver(outRect) && !Mouse.IsOver(new Rect(outRect.xMax - 16f, outRect.y, 16f, outRect.height)) && !Mouse.IsOver(new Rect(outRect.x, outRect.yMax - 16f, outRect.width, 16f)))
		{
			currentScrollView = outRect;
			currentScrollViewStartMousePos = UI.MousePositionOnUIInverted;
			scrollMouseTraveledEnoughDist = false;
			scrollVelocityRecords.Clear();
			if (scrollViewWithVelocity == currentScrollView)
			{
				scrollViewWithVelocity = null;
				scrollViewVelocity = default(Vector2);
			}
		}
		else if (Input.GetMouseButton(0) && currentScrollView == outRect)
		{
			if (!scrollMouseTraveledEnoughDist && (UI.MousePositionOnUIInverted - currentScrollViewStartMousePos).sqrMagnitude > 100f)
			{
				scrollMouseTraveledEnoughDist = true;
			}
			if (scrollMouseTraveledEnoughDist)
			{
				_ = UnityGUIBugsFixer.CurrentEventDelta;
				scrollPosition -= UnityGUIBugsFixer.CurrentEventDelta;
				scrollVelocityRecords.Add((Time.time, -UnityGUIBugsFixer.CurrentEventDelta));
				ClampScrollPosition(ref scrollPosition);
			}
		}
		else if (!Input.GetMouseButton(0) && currentScrollView == outRect)
		{
			if (scrollMouseTraveledEnoughDist)
			{
				scrollViewWithVelocity = currentScrollView;
				scrollViewVelocity = default(Vector2);
				for (int i = 0; i < scrollVelocityRecords.Count; i++)
				{
					if (Time.time - scrollVelocityRecords[i].Item1 < 0.057f)
					{
						scrollViewVelocity += scrollVelocityRecords[i].Item2;
					}
				}
				consumeAllMouseUpEventsOnFrame = Time.frameCount;
			}
			currentScrollView = null;
		}
		if (Event.current.type == EventType.Repaint)
		{
			Rect? rect = scrollViewWithVelocity;
			Rect rect2 = outRect;
			if (rect.HasValue && (!rect.HasValue || rect.GetValueOrDefault() == rect2) && scrollViewVelocity != default(Vector2))
			{
				scrollPosition += scrollViewVelocity;
				scrollViewVelocity = Vector2.MoveTowards(scrollViewVelocity, default(Vector2), Time.deltaTime * 90f);
				ClampScrollPosition(ref scrollPosition);
			}
		}
		if (consumeAllMouseUpEventsOnFrame == Time.frameCount && Event.current.type == EventType.MouseUp)
		{
			Event.current.Use();
		}
		static void ClampScrollPosition(ref Vector2 pos)
		{
			if (pos.x < 0f)
			{
				pos.x = 0f;
			}
			if (pos.y < 0f)
			{
				pos.y = 0f;
			}
		}
	}

	public static string GetKeyBindingLabel(KeyBindingDef keyDef)
	{
		switch (keyDef.MainKey)
		{
		case KeyCode.Return:
			return "A";
		case KeyCode.Escape:
			return "B";
		case KeyCode.Tab:
			return "X";
		case KeyCode.Space:
			return "Y";
		case KeyCode.PageDown:
			return "R1";
		case KeyCode.PageUp:
			return "L1";
		case KeyCode.W:
			return "↑";
		case KeyCode.A:
			return "←";
		case KeyCode.S:
			return "↓";
		case KeyCode.D:
			return "→";
		case KeyCode.Mouse0:
			return "R2";
		case KeyCode.Mouse1:
			return "L2";
		case KeyCode.Mouse2:
			return "L5";
		case KeyCode.LeftShift:
			return "R5";
		case KeyCode.RightShift:
			return "R5";
		case KeyCode.Home:
			return "R4";
		case KeyCode.End:
			return "L4";
		default:
			if (keyDef == KeyBindingDefOf.MapZoom_In || keyDef == KeyBindingDefOf.MapZoom_Out || keyDef == KeyBindingDefOf.Accept || keyDef == KeyBindingDefOf.Cancel || keyDef == KeyBindingDefOf.TogglePause || keyDef == KeyBindingDefOf.TimeSpeed_Faster || keyDef == KeyBindingDefOf.TimeSpeed_Slower || keyDef == KeyBindingDefOf.QueueOrder)
			{
				return "?";
			}
			return keyDef.MainKeyLabel;
		}
	}
}
