using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class UnityGUIBugsFixer
	{
		private static List<Resolution> resolutions = new List<Resolution>();

		private static Vector2 currentEventDelta;

		private static int lastMousePositionFrame;

		private const float ScrollFactor = 6f;

		private static Vector2? lastMousePosition;

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

		public static void OnGUI()
		{
			FixScrolling();
			FixShift();
			FixDelta();
		}

		private static void FixScrolling()
		{
			if (Event.current.type == EventType.ScrollWheel && (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer))
			{
				Vector2 delta = Event.current.delta;
				Event.current.delta = new Vector2(delta.x, delta.y * 6f);
			}
		}

		private static void FixShift()
		{
			if ((Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer) && !Event.current.shift)
			{
				Event.current.shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			}
		}

		public static bool ResolutionsEqual(IntVec2 a, IntVec2 b)
		{
			return a == b;
		}

		private static void FixDelta()
		{
			Vector2 vector = UI.GUIToScreenPoint(Event.current.mousePosition);
			if (Event.current.rawType == EventType.MouseDrag)
			{
				Vector2 value = vector;
				Vector2? rhs = lastMousePosition;
				if (value != rhs || Time.frameCount != lastMousePositionFrame)
				{
					if (lastMousePosition.HasValue)
					{
						currentEventDelta = vector - lastMousePosition.Value;
					}
					else
					{
						currentEventDelta = default(Vector2);
					}
					lastMousePosition = vector;
					lastMousePositionFrame = Time.frameCount;
				}
			}
			else
			{
				currentEventDelta = Event.current.delta;
				if (Event.current.rawType == EventType.MouseDown)
				{
					lastMousePosition = vector;
					lastMousePositionFrame = Time.frameCount;
				}
				else if (Event.current.rawType == EventType.MouseUp)
				{
					lastMousePosition = null;
				}
			}
		}
	}
}
