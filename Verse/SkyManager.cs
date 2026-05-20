using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SkyManager
{
	private readonly Map map;

	private float curSkyGlowInt;

	private SkyTarget curSky;

	private readonly List<Pair<SkyOverlay, float>> tempOverlays = new List<Pair<SkyOverlay, float>>();

	public const float NightMaxCelGlow = 0.1f;

	public const float DuskMaxCelGlow = 0.6f;

	private static readonly Color FogOfWarBaseColor = new Color32(77, 69, 66, byte.MaxValue);

	private readonly List<GameCondition> tempAllGameConditionsAffectingMap = new List<GameCondition>();

	private static readonly int LightsourceShineIntensity = Shader.PropertyToID("_LightsourceShineIntensity");

	private static readonly int LightsourceShineSizeReduction = Shader.PropertyToID("_LightsourceShineSizeReduction");

	private static readonly int DayPercent = Shader.PropertyToID("_DayPercent");

	public float CurSkyGlow => curSkyGlowInt;

	public SkyTarget CurSky => curSky;

	public SkyManager(Map map)
	{
		this.map = map;
	}

	public void SkyManagerUpdate()
	{
		curSky = CurrentSkyTarget();
		curSkyGlowInt = curSky.glow;
		if (map == Find.CurrentMap)
		{
			if (map.Biome != null && map.Biome.disableSkyLighting)
			{
				MatBases.LightOverlay.color = new Color(1f, 1f, 1f, 0f);
				MatBases.FogOfWar.color = map.FogOfWarColor ?? FogOfWarBaseColor;
			}
			else
			{
				MatBases.LightOverlay.color = curSky.colors.sky;
				Find.CameraColor.saturation = curSky.colors.saturation;
				Color sky = curSky.colors.sky;
				sky.a = 1f;
				sky *= map.FogOfWarColor ?? FogOfWarBaseColor;
				MatBases.FogOfWar.color = sky;
			}
			Color color = curSky.colors.shadow;
			Vector3? overridenShadowVector = GetOverridenShadowVector();
			if (overridenShadowVector.HasValue)
			{
				SetSunShadowVector(overridenShadowVector.Value);
			}
			else
			{
				SetSunShadowVector(GenCelestial.GetLightSourceInfo(map, GenCelestial.LightType.Shadow).vector);
				color = Color.Lerp(Color.white, color, GenCelestial.CurShadowStrength(map));
			}
			GenCelestial.LightInfo lightSourceInfo = GenCelestial.GetLightSourceInfo(map, GenCelestial.LightType.LightingSun);
			GenCelestial.LightInfo lightSourceInfo2 = GenCelestial.GetLightSourceInfo(map, GenCelestial.LightType.LightingMoon);
			Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectSun, new Vector4(lightSourceInfo.vector.x, 0f, lightSourceInfo.vector.y, lightSourceInfo.intensity));
			Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon, new Vector4(lightSourceInfo2.vector.x, 0f, lightSourceInfo2.vector.y, lightSourceInfo2.intensity));
			Shader.SetGlobalFloat(LightsourceShineSizeReduction, 20f * (1f / curSky.lightsourceShineSize));
			Shader.SetGlobalFloat(LightsourceShineIntensity, curSky.lightsourceShineIntensity);
			Shader.SetGlobalFloat(DayPercent, GenLocalDate.DayPercent(map));
			MatBases.SunShadow.color = color;
			MatBases.SunShadowFade.color = color;
			UpdateOverlays(curSky);
		}
	}

	public void ForceSetCurSkyGlow(float curSkyGlow)
	{
		curSkyGlowInt = curSkyGlow;
	}

	private void UpdateOverlays(SkyTarget curSky)
	{
		tempOverlays.Clear();
		List<SkyOverlay> overlays = map.weatherManager.curWeather.Worker.overlays;
		for (int i = 0; i < overlays.Count; i++)
		{
			AddTempOverlay(new Pair<SkyOverlay, float>(overlays[i], map.weatherManager.TransitionLerpFactor));
		}
		List<SkyOverlay> overlays2 = map.weatherManager.lastWeather.Worker.overlays;
		for (int j = 0; j < overlays2.Count; j++)
		{
			AddTempOverlay(new Pair<SkyOverlay, float>(overlays2[j], 1f - map.weatherManager.TransitionLerpFactor));
		}
		for (int k = 0; k < map.gameConditionManager.ActiveConditions.Count; k++)
		{
			GameCondition gameCondition = map.gameConditionManager.ActiveConditions[k];
			List<SkyOverlay> list = gameCondition.SkyOverlays(map);
			if (list != null)
			{
				for (int l = 0; l < list.Count; l++)
				{
					AddTempOverlay(new Pair<SkyOverlay, float>(list[l], gameCondition.SkyTargetLerpFactor(map)));
				}
			}
		}
		for (int m = 0; m < tempOverlays.Count; m++)
		{
			Color overlayColor = ((!tempOverlays[m].First.ForcedOverlayColor.HasValue) ? curSky.colors.overlay : tempOverlays[m].First.ForcedOverlayColor.Value);
			overlayColor.a = tempOverlays[m].Second;
			tempOverlays[m].First.SetOverlayColor(overlayColor);
		}
	}

	private void AddTempOverlay(Pair<SkyOverlay, float> pair)
	{
		for (int i = 0; i < tempOverlays.Count; i++)
		{
			if (tempOverlays[i].First == pair.First)
			{
				tempOverlays[i] = new Pair<SkyOverlay, float>(tempOverlays[i].First, Mathf.Clamp01(tempOverlays[i].Second + pair.Second));
				return;
			}
		}
		tempOverlays.Add(pair);
	}

	private void SetSunShadowVector(Vector2 vec)
	{
		Shader.SetGlobalVector(ShaderPropertyIDs.MapSunLightDirection, new Vector4(vec.x, 0f, vec.y, GenCelestial.CurShadowStrength(map)));
	}

	private SkyTarget CurrentSkyTarget()
	{
		SkyTarget b = map.weatherManager.curWeather.Worker.CurSkyTarget(map);
		SkyTarget skyTarget = SkyTarget.Lerp(map.weatherManager.lastWeather.Worker.CurSkyTarget(map), b, map.weatherManager.TransitionLerpFactor);
		map.gameConditionManager.GetAllGameConditionsAffectingMap(map, tempAllGameConditionsAffectingMap);
		for (int i = 0; i < tempAllGameConditionsAffectingMap.Count; i++)
		{
			SkyTarget? skyTarget2 = tempAllGameConditionsAffectingMap[i].SkyTarget(map);
			if (skyTarget2.HasValue)
			{
				skyTarget = SkyTarget.LerpDarken(skyTarget, skyTarget2.Value, tempAllGameConditionsAffectingMap[i].SkyTargetLerpFactor(map));
			}
		}
		tempAllGameConditionsAffectingMap.Clear();
		List<WeatherEvent> liveEventsListForReading = map.weatherManager.eventHandler.LiveEventsListForReading;
		for (int j = 0; j < liveEventsListForReading.Count; j++)
		{
			if (liveEventsListForReading[j].CurrentlyAffectsSky)
			{
				skyTarget = SkyTarget.Lerp(skyTarget, liveEventsListForReading[j].SkyTarget, liveEventsListForReading[j].SkyTargetLerpFactor);
			}
		}
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.AffectsSky);
		for (int k = 0; k < list.Count; k++)
		{
			CompAffectsSky compAffectsSky = list[k].TryGetComp<CompAffectsSky>();
			if (compAffectsSky.LerpFactor > 0f)
			{
				skyTarget = ((!compAffectsSky.Props.lerpDarken) ? SkyTarget.Lerp(skyTarget, compAffectsSky.SkyTarget, compAffectsSky.LerpFactor) : SkyTarget.LerpDarken(skyTarget, compAffectsSky.SkyTarget, compAffectsSky.LerpFactor));
			}
		}
		return skyTarget;
	}

	private Vector3? GetOverridenShadowVector()
	{
		List<WeatherEvent> liveEventsListForReading = map.weatherManager.eventHandler.LiveEventsListForReading;
		for (int i = 0; i < liveEventsListForReading.Count; i++)
		{
			Vector2? overrideShadowVector = liveEventsListForReading[i].OverrideShadowVector;
			if (overrideShadowVector.HasValue)
			{
				return overrideShadowVector;
			}
		}
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.AffectsSky);
		for (int j = 0; j < list.Count; j++)
		{
			Vector2? overrideShadowVector2 = list[j].TryGetComp<CompAffectsSky>().OverrideShadowVector;
			if (overrideShadowVector2.HasValue)
			{
				return overrideShadowVector2;
			}
		}
		return null;
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("SkyManager: ");
		stringBuilder.AppendLine("CurCelestialSunGlow: " + GenCelestial.CurCelestialSunGlow(Find.CurrentMap));
		stringBuilder.AppendLine("CurSkyGlow: " + CurSkyGlow.ToStringPercent());
		stringBuilder.AppendLine("CurrentSkyTarget: " + CurrentSkyTarget().ToString());
		return stringBuilder.ToString();
	}
}
