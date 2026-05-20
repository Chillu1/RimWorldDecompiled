using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_TorrentialRain : SkyOverlay
{
	private static readonly Material CloudLayer = MatLoader.LoadMat("Weather/TorrentialRain/CloudLayer");

	private static readonly Material ParticleLayer = MatLoader.LoadMat("Weather/TorrentialRain/ParticleLayer");

	private static readonly ComplexCurve speedCurve = new ComplexCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f), new UnityEngine.Keyframe(2f, 1.075f), new UnityEngine.Keyframe(3f, 1.15f));

	private TexturePannerSpeedCurve panner0 = new TexturePannerSpeedCurve(CloudLayer, "_MainTex", speedCurve, new Vector2(-0.25f, -1f), 0.05f);

	private TexturePannerSpeedCurve panner1 = new TexturePannerSpeedCurve(CloudLayer, "_MainTex2", speedCurve, new Vector2(-0.2f, -1f), 0.06f);

	private TexturePannerSpeedCurve panner2 = new TexturePannerSpeedCurve(ParticleLayer, "_MainTex", speedCurve, new Vector2(-0.34f, -1f), 0.06f);

	private TexturePannerSpeedCurve panner3 = new TexturePannerSpeedCurve(ParticleLayer, "_MainTex2", speedCurve, new Vector2(-0.2f, -1f), 0.05f);

	public override void DrawOverlay(Map map)
	{
		SkyOverlay.DrawWorldOverlay(map, CloudLayer);
		SkyOverlay.DrawWorldOverlay(map, ParticleLayer);
	}

	public override void SetOverlayColor(Color color)
	{
		CloudLayer.color = color;
		ParticleLayer.color = color;
	}

	public override void TickOverlay(Map map, float lerpFactor)
	{
		panner0.Tick();
		panner1.Tick();
		panner2.Tick();
		panner3.Tick();
	}
}
