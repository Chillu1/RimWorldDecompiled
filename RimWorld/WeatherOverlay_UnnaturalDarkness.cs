using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_UnnaturalDarkness : WeatherOverlayDualPanner
{
	private static readonly Material DarkParticlesOverlayWorld = MatLoader.LoadMat("Weather/DarknessOverlayWorld");

	public WeatherOverlay_UnnaturalDarkness()
	{
		worldOverlayMat = DarkParticlesOverlayWorld;
		worldOverlayPanSpeed1 = 0.004f;
		worldPanDir1 = new Vector2(-0.4f, -0.6f).normalized;
		worldOverlayPanSpeed2 = 0.005f;
		worldPanDir2 = new Vector2(0.2f, -0.8f).normalized;
	}
}
