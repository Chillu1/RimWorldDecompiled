using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class UI_BackgroundMain : UIMenuBackground
	{
		private Color curColor = new Color(1f, 1f, 1f, 0f);

		private Texture2D overlayImage;

		public Texture2D overrideBGImage;

		private bool fadeIn;

		private const float DeltaAlpha = 0.04f;

		private static readonly Vector2 BGPlanetSize = new Vector2(2048f, 1280f);

		private static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/HeroArt/BGPlanet");

		public override void BackgroundOnGUI()
		{
			Vector2 vector = ((overrideBGImage != null) ? new Vector2(overrideBGImage.width, overrideBGImage.height) : BGPlanetSize);
			bool flag = true;
			if ((float)UI.screenWidth > (float)UI.screenHeight * (vector.x / vector.y))
			{
				flag = false;
			}
			Rect rect;
			if (flag)
			{
				float height = UI.screenHeight;
				float num = (float)UI.screenHeight * (vector.x / vector.y);
				rect = new Rect((float)(UI.screenWidth / 2) - num / 2f, 0f, num, height);
			}
			else
			{
				float width = UI.screenWidth;
				float num2 = (float)UI.screenWidth * (vector.y / vector.x);
				rect = new Rect(0f, (float)(UI.screenHeight / 2) - num2 / 2f, width, num2);
			}
			GUI.DrawTexture(rect, overrideBGImage ?? BGPlanet, ScaleMode.ScaleToFit);
			DoOverlay(rect);
		}

		private void DoOverlay(Rect bgRect)
		{
			if (overlayImage != null)
			{
				if (fadeIn && curColor.a < 1f)
				{
					curColor.a += 0.04f;
				}
				else if (curColor.a > 0f)
				{
					curColor.a -= 0.04f;
				}
				curColor.a = Mathf.Clamp01(curColor.a);
				GUI.color = curColor;
				GUI.DrawTexture(bgRect, overlayImage, ScaleMode.ScaleAndCrop);
				GUI.color = Color.white;
			}
		}

		public void FadeOut()
		{
			fadeIn = false;
		}

		public void SetOverlayImage(Texture2D texture)
		{
			if (texture != null)
			{
				overlayImage = texture;
				fadeIn = true;
			}
		}
	}
}
