using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_SnowGentle : SkyOverlay
	{
		private static readonly Material SnowGentleOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld");

		public WeatherOverlay_SnowGentle()
		{
			worldOverlayMat = SnowGentleOverlayWorld;
			worldOverlayPanSpeed1 = 0.002f;
			worldPanDir1 = new Vector2(-0.25f, -1f);
			worldPanDir1.Normalize();
			worldOverlayPanSpeed2 = 0.003f;
			worldPanDir2 = new Vector2(-0.24f, -1f);
			worldPanDir2.Normalize();
		}
	}
}
