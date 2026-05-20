using System.Collections.Generic;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public static class UnityGUIBugsFixer
{
	private static List<Resolution> resolutions = new List<Resolution>();

	private static Vector2 currentEventDelta;

	private static int lastMousePositionFrame;

	private static bool leftMouseButtonPressed;

	private const float ScrollFactor = -6f;

	private static Vector2? lastMousePosition;

	public static bool IsSteamDeckOrLinuxBuild
	{
		get
		{
			if (!SteamDeck.IsSteamDeck && Application.platform != RuntimePlatform.LinuxEditor)
			{
				return Application.platform == RuntimePlatform.LinuxPlayer;
			}
			return true;
		}
	}

	public static List<Resolution> ScreenResolutionsWithoutDuplicates
	{
		get
		{
			resolutions.Clear();
			Resolution[] array = Screen.resolutions;
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = false;
				for (int j = 0; j < resolutions.Count; j++)
				{
					if (resolutions[j].width == array[i].width && resolutions[j].height == array[i].height)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					resolutions.Add(array[i]);
				}
			}
			return resolutions;
		}
	}

	public static Vector2 CurrentEventDelta => currentEventDelta;

	public static bool MouseDrag(int button = 0)
	{
		if (!IsSteamDeckOrLinuxBuild)
		{
			if (Event.current.type == EventType.MouseDrag)
			{
				return Event.current.button == button;
			}
			return false;
		}
		return Input.GetMouseButton(button);
	}

	public static void OnGUI()
	{
		RememberMouseStateForIsLeftMouseButtonPressed();
		FixSteamDeckMousePositionNeverUpdating();
		FixScrolling();
		FixShift();
		FixDelta();
		EnsureSliderDragReset();
	}

	private static void FixScrolling()
	{
		if (Event.current.type == EventType.ScrollWheel && (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer))
		{
			Vector2 delta = Event.current.delta;
			Event.current.delta = new Vector2(delta.x, delta.y * -6f);
		}
	}

	private static void FixShift()
	{
		if ((Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer) && !Event.current.shift)
		{
			Event.current.shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		}
	}

	public static bool ResolutionsEqual(IntVec2 a, IntVec2 b)
	{
		return a == b;
	}

	private static void FixDelta()
	{
		Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
		if (IsSteamDeckOrLinuxBuild)
		{
			if (Event.current.rawType == EventType.MouseDown)
			{
				lastMousePosition = mousePositionOnUIInverted;
				lastMousePositionFrame = Time.frameCount;
			}
			else if (Event.current.type == EventType.Repaint)
			{
				if (Time.frameCount != lastMousePositionFrame)
				{
					if (lastMousePosition.HasValue)
					{
						currentEventDelta = mousePositionOnUIInverted - lastMousePosition.Value;
					}
					else
					{
						currentEventDelta = default(Vector2);
					}
					lastMousePosition = mousePositionOnUIInverted;
					lastMousePositionFrame = Time.frameCount;
				}
			}
			else
			{
				currentEventDelta = default(Vector2);
			}
		}
		else if (Event.current.rawType == EventType.MouseDrag)
		{
			Vector2 vector = mousePositionOnUIInverted;
			Vector2? vector2 = lastMousePosition;
			if (!vector2.HasValue || vector != vector2.GetValueOrDefault() || Time.frameCount != lastMousePositionFrame)
			{
				if (lastMousePosition.HasValue)
				{
					currentEventDelta = mousePositionOnUIInverted - lastMousePosition.Value;
				}
				else
				{
					currentEventDelta = default(Vector2);
				}
				lastMousePosition = mousePositionOnUIInverted;
				lastMousePositionFrame = Time.frameCount;
			}
		}
		else
		{
			currentEventDelta = Event.current.delta;
			if (Event.current.rawType == EventType.MouseDown)
			{
				lastMousePosition = mousePositionOnUIInverted;
				lastMousePositionFrame = Time.frameCount;
			}
			else if (Event.current.rawType == EventType.MouseUp)
			{
				lastMousePosition = null;
			}
		}
	}

	private static void EnsureSliderDragReset()
	{
		if (Event.current.type == EventType.MouseUp)
		{
			Widgets.ResetSliderDraggingIDs();
		}
	}

	public static void Notify_BeginGroup()
	{
		FixSteamDeckMousePositionNeverUpdating();
	}

	public static void Notify_EndGroup()
	{
		FixSteamDeckMousePositionNeverUpdating();
	}

	public static void Notify_BeginScrollView()
	{
		FixSteamDeckMousePositionNeverUpdating();
	}

	public static void Notify_EndScrollView()
	{
		FixSteamDeckMousePositionNeverUpdating();
	}

	public static void Notify_GUIMatrixChanged()
	{
		FixSteamDeckMousePositionNeverUpdating();
	}

	private static void FixSteamDeckMousePositionNeverUpdating()
	{
		if (IsSteamDeckOrLinuxBuild)
		{
			Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
			mousePositionOnUIInverted = GUIUtility.ScreenToGUIPoint(mousePositionOnUIInverted * Prefs.UIScale);
			Event.current.mousePosition = mousePositionOnUIInverted;
		}
	}

	private static void RememberMouseStateForIsLeftMouseButtonPressed()
	{
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			leftMouseButtonPressed = true;
		}
		else if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
		{
			leftMouseButtonPressed = false;
		}
	}

	public static bool IsLeftMouseButtonPressed()
	{
		if (Input.GetMouseButton(0))
		{
			return true;
		}
		return leftMouseButtonPressed;
	}
}
