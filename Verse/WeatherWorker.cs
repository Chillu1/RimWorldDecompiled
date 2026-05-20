using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class WeatherWorker
{
	private struct SkyThreshold
	{
		public SkyColorSet colors;

		public float celGlowThreshold;

		public SkyThreshold(SkyColorSet colors, float celGlowThreshold)
		{
			this.colors = colors;
			this.celGlowThreshold = celGlowThreshold;
		}
	}

	private WeatherDef def;

	public List<SkyOverlay> overlays = new List<SkyOverlay>();

	private SkyThreshold[] skyTargets = new SkyThreshold[4];

	private const float LerpDeltaMin = 0.1f;

	private const float LerpDeltaMax = 0.6f;

	private const float LerpStep = 0.002f;

	public WeatherWorker(WeatherDef def)
	{
		this.def = def;
		foreach (Type overlayClass in def.overlayClasses)
		{
			SkyOverlay item = (SkyOverlay)GenGeneric.InvokeStaticGenericMethod(typeof(WeatherPartPool), overlayClass, "GetInstanceOf");
			overlays.Add(item);
		}
		skyTargets[0] = new SkyThreshold(def.skyColorsNightMid, 0f);
		skyTargets[1] = new SkyThreshold(def.skyColorsNightEdge, 0.1f);
		skyTargets[2] = new SkyThreshold(def.skyColorsDusk, 0.6f);
		skyTargets[3] = new SkyThreshold(def.skyColorsDay, 1f);
	}

	public virtual void OnWeatherStart(Map map)
	{
		for (int i = 0; i < overlays.Count; i++)
		{
			overlays[i].Reset();
		}
		if (def.letterDef != null)
		{
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(def.letterLabel, def.letterText, def.letterDef);
			Find.LetterStack.ReceiveLetter(choiceLetter);
		}
	}

	public virtual void OnWeatherEnd(Map map)
	{
		for (int i = 0; i < overlays.Count; i++)
		{
			overlays[i].Reset();
		}
	}

	public void DrawWeather(Map map)
	{
		for (int i = 0; i < overlays.Count; i++)
		{
			overlays[i].DrawOverlay(map);
		}
	}

	public void WeatherTick(Map map, float lerpFactor)
	{
		for (int i = 0; i < overlays.Count; i++)
		{
			overlays[i].TickOverlay(map, lerpFactor);
		}
		for (int j = 0; j < def.eventMakers.Count; j++)
		{
			def.eventMakers[j].WeatherEventMakerTick(map, lerpFactor);
		}
		if (!def.doToxicBuildup || Find.TickManager.TicksGame % 3451 != 0 || !Mathf.Approximately(lerpFactor, 1f))
		{
			return;
		}
		IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
		for (int k = 0; k < allPawnsSpawned.Count; k++)
		{
			if (!allPawnsSpawned[k].kindDef.immuneToGameConditionEffects)
			{
				ToxicUtility.DoAirbornePawnToxicDamage(allPawnsSpawned[k]);
			}
		}
	}

	public SkyTarget CurSkyTarget(Map map)
	{
		float num = GenCelestial.CurCelestialSunGlow(map);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < skyTargets.Length; i++)
		{
			num3 = i;
			if (num + 0.001f < skyTargets[i].celGlowThreshold)
			{
				break;
			}
			num2 = i;
		}
		SkyThreshold skyThreshold = skyTargets[num2];
		SkyThreshold skyThreshold2 = skyTargets[num3];
		float num4 = skyThreshold2.celGlowThreshold - skyThreshold.celGlowThreshold;
		float num5 = ((num4 != 0f) ? ((num - skyThreshold.celGlowThreshold) / num4) : 1f);
		if (Find.WorldGrid.LongLatOf(map.Tile).y >= 75f)
		{
			num5 = ClampLerpDelta(map, num5);
		}
		SkyTarget result = new SkyTarget
		{
			glow = Math.Min(num, def.maxGlow),
			colors = SkyColorSet.Lerp(skyThreshold.colors, skyThreshold2.colors, num5)
		};
		if (GenCelestial.IsDaytime(num))
		{
			result.lightsourceShineIntensity = 1f;
			result.lightsourceShineSize = 1f;
		}
		else
		{
			result.lightsourceShineIntensity = 0.7f;
			result.lightsourceShineSize = 0.5f;
		}
		return result;
	}

	private static float ClampLerpDelta(Map map, float lerpVal)
	{
		WeatherManager weatherManager = map.weatherManager;
		if (Mathf.Approximately(weatherManager.prevSkyTargetLerp, weatherManager.currSkyTargetLerp) && Mathf.Approximately(weatherManager.currSkyTargetLerp, -1f))
		{
			weatherManager.prevSkyTargetLerp = (weatherManager.currSkyTargetLerp = lerpVal);
		}
		else
		{
			weatherManager.prevSkyTargetLerp = weatherManager.currSkyTargetLerp;
			weatherManager.currSkyTargetLerp = lerpVal;
			float num = Mathf.Abs(weatherManager.prevSkyTargetLerp - weatherManager.currSkyTargetLerp);
			if (num > 0.1f && num < 0.6f && !Mathf.Approximately(weatherManager.prevSkyTargetLerp, -1f))
			{
				lerpVal = ((weatherManager.prevSkyTargetLerp > weatherManager.currSkyTargetLerp) ? (weatherManager.prevSkyTargetLerp - 0.002f) : (weatherManager.prevSkyTargetLerp + 0.002f));
				weatherManager.currSkyTargetLerp = lerpVal;
			}
		}
		return lerpVal;
	}
}
