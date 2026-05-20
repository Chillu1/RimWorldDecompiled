using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_GrayPallAsh : WeatherOverlayDualPanner
{
	private static readonly Material DarkParticlesOverlayWorld = MatLoader.LoadMat("Weather/GraypallAshesOverlayWorld");

	public WeatherOverlay_GrayPallAsh()
	{
		worldOverlayMat = DarkParticlesOverlayWorld;
		worldOverlayPanSpeed1 = 0.003f;
		worldPanDir1 = new Vector2(-0.12f, -1f);
		worldPanDir1.Normalize();
		worldOverlayPanSpeed2 = 0.001f;
		worldPanDir2 = new Vector2(0.12f, -1f);
		worldPanDir2.Normalize();
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			SetOverlayColor(new Color(1f, 1f, 1f, 0.5f));
		});
	}
}
