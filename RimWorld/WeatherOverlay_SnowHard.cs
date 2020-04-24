using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_SnowHard : SkyOverlay
	{
		private static readonly Material SnowOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld");

		public WeatherOverlay_SnowHard()
		{
			worldOverlayMat = SnowOverlayWorld;
			worldOverlayPanSpeed1 = 0.008f;
			worldPanDir1 = new Vector2(-0.5f, -1f);
			worldPanDir1.Normalize();
			worldOverlayPanSpeed2 = 0.009f;
			worldPanDir2 = new Vector2(-0.48f, -1f);
			worldPanDir2.Normalize();
		}
	}
}
