using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_Fallout : SkyOverlay
	{
		private static readonly Material FalloutOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld");

		public WeatherOverlay_Fallout()
		{
			worldOverlayMat = FalloutOverlayWorld;
			worldOverlayPanSpeed1 = 0.0008f;
			worldPanDir1 = new Vector2(-0.25f, -1f);
			worldPanDir1.Normalize();
			worldOverlayPanSpeed2 = 0.0012f;
			worldPanDir2 = new Vector2(-0.24f, -1f);
			worldPanDir2.Normalize();
		}
	}
}
