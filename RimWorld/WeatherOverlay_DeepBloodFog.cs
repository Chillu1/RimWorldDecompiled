using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_DeepBloodFog : WeatherOverlayDualPanner
{
	private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/DeepBloodFogOverlayWorld");

	public WeatherOverlay_DeepBloodFog()
	{
		worldOverlayMat = FogOverlayWorld;
		worldOverlayPanSpeed1 = 0.001f;
		worldOverlayPanSpeed2 = 0.002f;
		worldPanDir1 = new Vector2(1f, 1f);
		worldPanDir1.Normalize();
		worldPanDir2 = new Vector2(0.5f, -0.1f);
		worldPanDir2.Normalize();
		base.ForcedOverlayColor = new Color(0.6f, 0f, 0f);
	}
}
