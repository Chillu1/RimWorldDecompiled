using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GameCondition_VolcanicAsh : GameCondition
{
	[StaticConstructorOnStartup]
	private class VolcanicAshOverlay : WeatherOverlayDualPanner
	{
		private static readonly Material CloudLayer = MatLoader.LoadMat("Weather/VolcanicAsh/CloudLayer");

		private static readonly Material ParticleLayer = MatLoader.LoadMat("Weather/VolcanicAsh/ParticleLayer");

		private static readonly ComplexCurve speedCurve = new ComplexCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f), new UnityEngine.Keyframe(2f, 1.15f), new UnityEngine.Keyframe(3f, 1.3f));

		private TexturePannerSpeedCurve panner0 = new TexturePannerSpeedCurve(CloudLayer, "_MainTex", speedCurve, new Vector2(1f, -0.2f), 0.007f);

		private TexturePannerSpeedCurve panner1 = new TexturePannerSpeedCurve(CloudLayer, "_MainTex2", speedCurve, new Vector2(1f, -0.28f), 0.002f);

		private TexturePannerSpeedCurve panner2 = new TexturePannerSpeedCurve(ParticleLayer, "_MainTex", speedCurve, new Vector2(-0.28f, -1f), 0.0025f);

		private TexturePannerSpeedCurve panner3 = new TexturePannerSpeedCurve(ParticleLayer, "_MainTex2", speedCurve, new Vector2(-0.1f, -1f), 0.002f);

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

	private static readonly SkyColorSet AshSkyColors = new SkyColorSet(new Color(0.482f, 0.403f, 0.402f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);

	private static readonly VolcanicAshOverlay volcanicAshOverlay = new VolcanicAshOverlay();

	private static readonly List<SkyOverlay> overlays = new List<SkyOverlay> { volcanicAshOverlay };

	private const int CheckInterval = 3251;

	public override int TransitionTicks => 200;

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
	}

	public override SkyTarget? SkyTarget(Map map)
	{
		return new SkyTarget(0f, AshSkyColors, 1f, 0f);
	}

	public override void Init()
	{
		if (!ModLister.CheckOdyssey("Volcanic ash"))
		{
			End();
		}
		else
		{
			base.Init();
		}
	}

	public override List<SkyOverlay> SkyOverlays(Map map)
	{
		return overlays;
	}

	public override void GameConditionDraw(Map map)
	{
		if (!HiddenByOtherCondition(map))
		{
			volcanicAshOverlay.DrawOverlay(map);
		}
	}

	public override void GameConditionTick()
	{
		if (Find.TickManager.TicksGame % 3251 == 0)
		{
			foreach (Map affectedMap in base.AffectedMaps)
			{
				foreach (Pawn item in affectedMap.mapPawns.AllPawnsSpawned)
				{
					if (!item.Position.Roofed(affectedMap) && !item.kindDef.immuneToGameConditionEffects && GasUtility.IsAffectedByExposure(item))
					{
						GiveOrUpdateHediff(item);
					}
				}
			}
		}
		foreach (Map affectedMap2 in base.AffectedMaps)
		{
			if (!HiddenByOtherCondition(affectedMap2))
			{
				volcanicAshOverlay.TickOverlay(affectedMap2, 1f);
			}
		}
	}

	private void GiveOrUpdateHediff(Pawn target)
	{
		Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VolcanicAsh);
		if (hediff == null)
		{
			hediff = target.health.AddHediff(HediffDefOf.VolcanicAsh);
		}
		hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 3256;
	}
}
