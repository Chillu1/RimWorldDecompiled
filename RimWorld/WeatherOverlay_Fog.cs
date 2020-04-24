using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_Fog : SkyOverlay
	{
		private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/FogOverlayWorld");

		public WeatherOverlay_Fog()
		{
			worldOverlayMat = FogOverlayWorld;
			worldOverlayPanSpeed1 = 0.0005f;
			worldOverlayPanSpeed2 = 0.0004f;
			worldPanDir1 = new Vector2(1f, 1f);
			worldPanDir2 = new Vector2(1f, -1f);
		}
	}
}
