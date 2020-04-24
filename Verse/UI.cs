using UnityEngine;

namespace Verse
{
	public static class UI
	{
		public static int screenWidth;

		public static int screenHeight;

		public static Vector2 MousePositionOnUI => Input.mousePosition / Prefs.UIScale;

		public static Vector2 MousePositionOnUIInverted
		{
			get
			{
				Vector2 mousePositionOnUI = MousePositionOnUI;
				mousePositionOnUI.y = (float)screenHeight - mousePositionOnUI.y;
				return mousePositionOnUI;
			}
		}

		public static Vector2 MousePosUIInvertedUseEventIfCan
		{
			get
			{
				if (Event.current != null)
				{
					return GUIToScreenPoint(Event.current.mousePosition);
				}
				return MousePositionOnUIInverted;
			}
		}

		public static void ApplyUIScale()
		{
			if (Prefs.UIScale == 1f)
			{
				screenWidth = Screen.width;
				screenHeight = Screen.height;
				return;
			}
			screenWidth = Mathf.RoundToInt((float)Screen.width / Prefs.UIScale);
			screenHeight = Mathf.RoundToInt((float)Screen.height / Prefs.UIScale);
			float uIScale = Prefs.UIScale;
			float uIScale2 = Prefs.UIScale;
			GUI.matrix = Matrix4x4.TRS(new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(uIScale, uIScale2, 1f));
		}

		public static void FocusControl(string controlName, Window window)
		{
			GUI.FocusControl(controlName);
			Find.WindowStack.Notify_ManuallySetFocus(window);
		}

		public static void UnfocusCurrentControl()
		{
			GUI.FocusControl(null);
		}

		public static Vector2 GUIToScreenPoint(Vector2 guiPoint)
		{
			return GUIUtility.GUIToScreenPoint(guiPoint / Prefs.UIScale);
		}

		public static void RotateAroundPivot(float angle, Vector2 center)
		{
			GUIUtility.RotateAroundPivot(angle, center * Prefs.UIScale);
		}

		public static Vector2 MapToUIPosition(this Vector3 v)
		{
			Vector3 vector = Find.Camera.WorldToScreenPoint(v) / Prefs.UIScale;
			return new Vector2(vector.x, (float)screenHeight - vector.y);
		}

		public static Vector3 UIToMapPosition(float x, float y)
		{
			return UIToMapPosition(new Vector2(x, y));
		}

		public static Vector3 UIToMapPosition(Vector2 screenLoc)
		{
			Ray ray = Find.Camera.ScreenPointToRay(screenLoc * Prefs.UIScale);
			return new Vector3(ray.origin.x, 0f, ray.origin.z);
		}

		public static float CurUICellSize()
		{
			return (new Vector3(1f, 0f, 0f).MapToUIPosition() - new Vector3(0f, 0f, 0f).MapToUIPosition()).x;
		}

		public static Vector3 MouseMapPosition()
		{
			return UIToMapPosition(MousePositionOnUI);
		}

		public static IntVec3 MouseCell()
		{
			return UIToMapPosition(MousePositionOnUI).ToIntVec3();
		}
	}
}
