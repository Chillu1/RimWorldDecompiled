using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_NoxiousHaze : WeatherOverlayDualPanner
{
	private static readonly Material NoxiusHazeOverlayWorld = MatLoader.LoadMat("Weather/NoxiousHazeOverlayWorld");

	public WeatherOverlay_NoxiousHaze()
	{
		worldOverlayMat = NoxiusHazeOverlayWorld;
		worldOverlayPanSpeed1 = 0.0004f;
		worldOverlayPanSpeed2 = 0.0003f;
		worldPanDir1 = new Vector2(1f, 1f);
		worldPanDir2 = new Vector2(1f, -1f);
	}
}
