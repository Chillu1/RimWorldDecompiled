using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_BloodRain : WeatherOverlayDualPanner
{
	private static readonly Material BloodRainMat = MatLoader.LoadMat("Weather/BloodRainOverlayWorld");

	public WeatherOverlay_BloodRain()
	{
		worldOverlayMat = BloodRainMat;
		worldOverlayPanSpeed1 = 0.015f;
		worldPanDir1 = new Vector2(-0.25f, -1f);
		worldPanDir1.Normalize();
		worldOverlayPanSpeed2 = 0.022f;
		worldPanDir2 = new Vector2(-0.24f, -1f);
		worldPanDir2.Normalize();
		base.ForcedOverlayColor = new Color(0.6f, 0f, 0f);
	}
}
