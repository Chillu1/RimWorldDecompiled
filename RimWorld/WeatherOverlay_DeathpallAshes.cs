using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_DeathpallAshes : WeatherOverlayDualPanner
{
	private static readonly Material AshesMat = MatLoader.LoadMat("Weather/DeathpallAshesOverlayWorld");

	public WeatherOverlay_DeathpallAshes()
	{
		worldOverlayMat = AshesMat;
		worldOverlayPanSpeed1 = 0.003f;
		worldPanDir1 = new Vector2(-0.12f, -1f);
		worldPanDir1.Normalize();
		worldOverlayPanSpeed2 = 0.001f;
		worldPanDir2 = new Vector2(0.12f, -1f);
		worldPanDir2.Normalize();
		base.ForcedOverlayColor = new Color(0.5f, 0.5f, 0.5f);
	}
}
