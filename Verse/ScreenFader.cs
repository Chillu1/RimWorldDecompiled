using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ScreenFader
	{
		private static GUIStyle backgroundStyle;

		private static Texture2D fadeTexture;

		private static Color sourceColor;

		private static Color targetColor;

		private static float sourceTime;

		private static float targetTime;

		private static bool fadeTextureDirty;

		private static float CurTime => Time.realtimeSinceStartup;

		static ScreenFader()
		{
			backgroundStyle = new GUIStyle();
			sourceColor = new Color(0f, 0f, 0f, 0f);
			targetColor = new Color(0f, 0f, 0f, 0f);
			sourceTime = 0f;
			targetTime = 0f;
			fadeTextureDirty = true;
			fadeTexture = new Texture2D(1, 1);
			fadeTexture.name = "ScreenFader";
			backgroundStyle.normal.background = fadeTexture;
			fadeTextureDirty = true;
		}

		public static void OverlayOnGUI(Vector2 windowSize)
		{
			Color color = CurrentInstantColor();
			if (color.a > 0f)
			{
				if (fadeTextureDirty)
				{
					fadeTexture.SetPixel(0, 0, color);
					fadeTexture.Apply();
				}
				GUI.Label(new Rect(-10f, -10f, windowSize.x + 10f, windowSize.y + 10f), fadeTexture, backgroundStyle);
			}
		}

		private static Color CurrentInstantColor()
		{
			if (CurTime > targetTime || targetTime == sourceTime)
			{
				return targetColor;
			}
			return Color.Lerp(sourceColor, targetColor, (CurTime - sourceTime) / (targetTime - sourceTime));
		}

		public static void SetColor(Color newColor)
		{
			sourceColor = newColor;
			targetColor = newColor;
			targetTime = 0f;
			sourceTime = 0f;
			fadeTextureDirty = true;
		}

		public static void StartFade(Color finalColor, float duration)
		{
			if (duration <= 0f)
			{
				SetColor(finalColor);
				return;
			}
			sourceColor = CurrentInstantColor();
			targetColor = finalColor;
			sourceTime = CurTime;
			targetTime = CurTime + duration;
		}
	}
}
