using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GameCondition_BioluminescentSpores : GameCondition
{
	[StaticConstructorOnStartup]
	private class GlowSporeOverlay : SkyOverlay
	{
		private static readonly Material GlowSporeOverlayWorld = MatLoader.LoadMat("Weather/GlowSporeOverlayWorld");

		private static readonly ComplexCurve speedCurve = new ComplexCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f), new UnityEngine.Keyframe(2f, 1.1f), new UnityEngine.Keyframe(3f, 1.2f));

		private TexturePannerSpeedCurve panner0 = new TexturePannerSpeedCurve(GlowSporeOverlayWorld, "_MainTex", speedCurve, new Vector2(-1f, -0.2f), 0.0001f);

		private TexturePannerSpeedCurve panner1 = new TexturePannerSpeedCurve(GlowSporeOverlayWorld, "_MainTex2", speedCurve, new Vector2(0.35f, -1f), 5E-05f);

		public override void SetOverlayColor(Color color)
		{
			GlowSporeOverlayWorld.color = color;
		}

		public override void DrawOverlay(Map map)
		{
			SkyOverlay.DrawWorldOverlay(map, GlowSporeOverlayWorld);
		}

		public override void TickOverlay(Map map, float lerpFactor)
		{
			panner0.Tick();
			panner1.Tick();
		}
	}

	private int curColorIndex = -1;

	private int prevColorIndex = -1;

	private float curColorTransition;

	private const float SkyColorStrength = 0.075f;

	private const float OverlayColorStrength = 0.025f;

	private const float BaseBrightness = 0.73f;

	private const int TransitionDurationTicks_NotPermanent = 280;

	private static readonly GlowSporeOverlay glowSporeOverlay = new GlowSporeOverlay();

	private static readonly List<SkyOverlay> overlays = new List<SkyOverlay> { glowSporeOverlay };

	private static readonly Color[] Colors = new Color[8]
	{
		new Color(0f, 1f, 0f),
		new Color(0.3f, 1f, 0f),
		new Color(0f, 1f, 0.7f),
		new Color(0.3f, 1f, 0.7f),
		new Color(0f, 0.5f, 1f),
		new Color(0f, 0f, 1f),
		new Color(0.87f, 0f, 1f),
		new Color(0.75f, 0f, 1f)
	};

	public Color CurrentColor => Color.Lerp(Colors[prevColorIndex], Colors[curColorIndex], curColorTransition);

	private int TransitionDurationTicks => 280;

	public override int TransitionTicks => 200;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref curColorIndex, "curColorIndex", 0);
		Scribe_Values.Look(ref prevColorIndex, "prevColorIndex", 0);
		Scribe_Values.Look(ref curColorTransition, "curColorTransition", 0f);
	}

	public override void Init()
	{
		base.Init();
		curColorIndex = Rand.Range(0, Colors.Length);
		prevColorIndex = curColorIndex;
		curColorTransition = 1f;
	}

	public override float SkyGazeChanceFactor(Map map)
	{
		return 8f;
	}

	public override float SkyGazeJoyGainFactor(Map map)
	{
		return 5f;
	}

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
	}

	public override SkyTarget? SkyTarget(Map map)
	{
		Color currentColor = CurrentColor;
		SkyColorSet colorSet = new SkyColorSet(Color.Lerp(Color.white, currentColor, 0.075f) * Brightness(map), new Color(0.92f, 0.92f, 0.92f), Color.Lerp(Color.white, currentColor, 0.025f) * Brightness(map), 1f);
		return new SkyTarget(0f, colorSet, 1f, 1f);
	}

	private float Brightness(Map map)
	{
		return Mathf.Max(0.73f, GenCelestial.CurCelestialSunGlow(map));
	}

	public override void GameConditionDraw(Map map)
	{
		if (!HiddenByOtherCondition(map))
		{
			glowSporeOverlay.DrawOverlay(map);
		}
	}

	public override void GameConditionTick()
	{
		curColorTransition += 1f / (float)TransitionDurationTicks;
		if (curColorTransition >= 1f)
		{
			prevColorIndex = curColorIndex;
			curColorIndex = GetNewColorIndex();
			curColorTransition = 0f;
		}
		foreach (Map affectedMap in base.AffectedMaps)
		{
			if (!HiddenByOtherCondition(affectedMap))
			{
				glowSporeOverlay.TickOverlay(affectedMap, 1f);
			}
		}
	}

	private int GetNewColorIndex()
	{
		return (from x in Enumerable.Range(0, Colors.Length)
			where x != curColorIndex
			select x).RandomElement();
	}

	public override List<SkyOverlay> SkyOverlays(Map map)
	{
		return overlays;
	}
}
