using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_Rain : SkyOverlay
	{
		private static readonly Material RainOverlayWorld = MatLoader.LoadMat("Weather/RainOverlayWorld");

		public WeatherOverlay_Rain()
		{
			worldOverlayMat = RainOverlayWorld;
			worldOverlayPanSpeed1 = 0.015f;
			worldPanDir1 = new Vector2(-0.25f, -1f);
			worldPanDir1.Normalize();
			worldOverlayPanSpeed2 = 0.022f;
			worldPanDir2 = new Vector2(-0.24f, -1f);
			worldPanDir2.Normalize();
		}
	}
}
