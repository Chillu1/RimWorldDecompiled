using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_GrayPallFog : WeatherOverlayDualPanner
{
	private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/UnnaturalFogOverlayWorld");

	public WeatherOverlay_GrayPallFog()
	{
		worldOverlayMat = FogOverlayWorld;
		worldOverlayPanSpeed1 = 0.001f;
		worldOverlayPanSpeed2 = 0.0005f;
		worldPanDir1 = new Vector2(1f, 0.2f);
		worldPanDir1.Normalize();
		worldPanDir2 = new Vector2(-1f, -0.2f);
		worldPanDir2.Normalize();
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			SetOverlayColor(new Color(1f, 1f, 1f, 1f));
		});
	}
}
