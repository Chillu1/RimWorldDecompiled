using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class WeatherOverlay_SnowBlizzard : SkyOverlay
{
	private Color color;

	private static readonly Material CloudLayer = MatLoader.LoadMat("Weather/SnowBlizzard/CloudLayer");

	private static readonly Material ParticleLayer = MatLoader.LoadMat("Weather/SnowBlizzard/ParticleLayer");

	private static readonly ComplexCurve speedCurve = new ComplexCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f), new UnityEngine.Keyframe(2f, 1.05f), new UnityEngine.Keyframe(3f, 1.1f));

	private TexturePannerSpeedCurve panner0 = new TexturePannerSpeedCurve(CloudLayer, "_MainTex", speedCurve, new Vector2(-1f, -0.3f), 0.03f);

	private TexturePannerSpeedCurve panner1 = new TexturePannerSpeedCurve(CloudLayer, "_MainTex2", speedCurve, new Vector2(-1f, -0.28f), 0.06f);

	private TexturePannerSpeedCurve panner2 = new TexturePannerSpeedCurve(ParticleLayer, "_MainTex", speedCurve, new Vector2(-0.5f, -0.2f), 0.05f);

	private TexturePannerSpeedCurve panner3 = new TexturePannerSpeedCurve(ParticleLayer, "_MainTex2", speedCurve, new Vector2(-0.5f, -0.3f), 0.06f);

	public override void DrawOverlay(Map map)
	{
		SkyOverlay.DrawWorldOverlay(map, ParticleLayer);
		SkyOverlay.DrawWorldOverlay(map, CloudLayer);
	}

	public override void SetOverlayColor(Color color)
	{
		this.color = color;
	}

	public override void TickOverlay(Map map, float lerpFactor)
	{
		float num = 0.5f + 0.5f * GenCelestial.CurCelestialSunGlow(map);
		Color color = new Color(this.color.r * num, this.color.g * num, this.color.b * num, this.color.a);
		CloudLayer.color = color;
		ParticleLayer.color = color;
		panner0.Tick();
		panner1.Tick();
		panner2.Tick();
		panner3.Tick();
	}
}
