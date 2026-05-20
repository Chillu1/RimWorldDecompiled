using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_HateChant : WeatherOverlayDualPanner
{
	private static readonly Material HateChantOverlayWorld = MatLoader.LoadMat("Weather/HateChantOverlayWorld");

	public WeatherOverlay_HateChant()
	{
		worldOverlayMat = HateChantOverlayWorld;
	}
}
