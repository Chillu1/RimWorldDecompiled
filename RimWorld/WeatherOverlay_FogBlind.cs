using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_FogBlind : WeatherOverlayDualPanner
{
	private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/BlindFogOverlayWorld");

	public WeatherOverlay_FogBlind()
	{
		worldOverlayMat = FogOverlayWorld;
		worldOverlayPanSpeed1 = 5E-05f;
		worldOverlayPanSpeed2 = 4E-05f;
		worldPanDir1 = new Vector2(1f, 1f);
		worldPanDir2 = new Vector2(1f, -1f);
	}
}
