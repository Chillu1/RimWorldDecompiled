using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public sealed class WeatherManager : IExposable
{
	public Map map;

	public readonly WeatherEventHandler eventHandler = new WeatherEventHandler();

	public WeatherDef curWeather = WeatherDefOf.Clear;

	public WeatherDef lastWeather = WeatherDefOf.Clear;

	public float prevSkyTargetLerp = -1f;

	public float currSkyTargetLerp = -1f;

	public int curWeatherAge;

	private readonly Dictionary<SoundDef, Sustainer> ambienceSustainers = new Dictionary<SoundDef, Sustainer>();

	public const float TransitionTicks = 4000f;

	private bool tickedLastWeather;

	private static readonly HashSet<SoundDef> TmpToRemove = new HashSet<SoundDef>();

	public float TransitionLerpFactor
	{
		get
		{
			float num = (float)curWeatherAge / 4000f;
			if (num > 1f)
			{
				num = 1f;
			}
			return num;
		}
	}

	public float RainRate => Mathf.Lerp(lastWeather.rainRate, curWeather.rainRate, TransitionLerpFactor);

	public float SnowRate => Mathf.Lerp(lastWeather.snowRate, curWeather.snowRate, TransitionLerpFactor);

	public float SandRate
	{
		get
		{
			if (!ModsConfig.OdysseyActive)
			{
				return 0f;
			}
			return Mathf.Lerp(lastWeather.sandRate, curWeather.sandRate, TransitionLerpFactor);
		}
	}

	public float CurWindSpeedFactor => Mathf.Lerp(lastWeather.windSpeedFactor, curWeather.windSpeedFactor, TransitionLerpFactor);

	public float CurWindSpeedOffset => Mathf.Lerp(lastWeather.windSpeedOffset, curWeather.windSpeedOffset, TransitionLerpFactor);

	public float CurMoveSpeedMultiplier => Mathf.Lerp(lastWeather.moveSpeedMultiplier, curWeather.moveSpeedMultiplier, TransitionLerpFactor);

	public float CurWeatherAccuracyMultiplier => Mathf.Lerp(lastWeather.accuracyMultiplier, curWeather.accuracyMultiplier, TransitionLerpFactor);

	public float CurWeatherMaxRangeCap => curWeather.maxRangeCap;

	public WeatherDef CurWeatherPerceived
	{
		get
		{
			if (curWeather == null)
			{
				return lastWeather;
			}
			if (lastWeather == null)
			{
				return curWeather;
			}
			float num = ((curWeather.perceivePriority > lastWeather.perceivePriority) ? 0.18f : ((!(lastWeather.perceivePriority > curWeather.perceivePriority)) ? 0.5f : 0.82f));
			if (!(TransitionLerpFactor < num))
			{
				return curWeather;
			}
			return lastWeather;
		}
	}

	public WeatherDef CurWeatherLerped
	{
		get
		{
			if (curWeather == null)
			{
				return lastWeather;
			}
			if (lastWeather == null)
			{
				return curWeather;
			}
			if (!(TransitionLerpFactor < 0.5f))
			{
				return curWeather;
			}
			return lastWeather;
		}
	}

	public WeatherManager(Map map)
	{
		this.map = map;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref curWeatherAge, "curWeatherAge", 0, forceSave: true);
		Scribe_Values.Look(ref prevSkyTargetLerp, "prevSkyTargetLerp", -1f);
		Scribe_Values.Look(ref currSkyTargetLerp, "currSkyTargetLerp", -1f);
		Scribe_Defs.Look(ref curWeather, "curWeather");
		Scribe_Defs.Look(ref lastWeather, "lastWeather");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			ambienceSustainers.Clear();
		}
	}

	public void TransitionTo(WeatherDef newWeather)
	{
		lastWeather = curWeather;
		curWeather = newWeather;
		lastWeather.Worker.OnWeatherEnd(map);
		ResetSkyTargetLerpCache();
		curWeatherAge = 0;
		if (lastWeather != newWeather)
		{
			newWeather.Worker.OnWeatherStart(map);
		}
	}

	public void DoWeatherGUI(Rect rect)
	{
		WeatherDef curWeatherPerceived = CurWeatherPerceived;
		Text.Anchor = TextAnchor.MiddleRight;
		Rect rect2 = new Rect(rect);
		rect2.width -= 15f;
		Text.Font = GameFont.Small;
		Widgets.Label(rect2, curWeatherPerceived.LabelCap);
		if (!curWeatherPerceived.description.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, curWeatherPerceived.description);
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public void WeatherManagerTick()
	{
		eventHandler.WeatherEventHandlerTick();
		curWeatherAge++;
		curWeather.Worker.WeatherTick(map, TransitionLerpFactor);
		if (TransitionLerpFactor < 1f || !tickedLastWeather)
		{
			lastWeather.Worker.WeatherTick(map, 1f - TransitionLerpFactor);
			tickedLastWeather = true;
		}
		AmbientSoundsTick();
	}

	private void AmbientSoundsTick()
	{
		foreach (SoundDef ambientSound in curWeather.ambientSounds)
		{
			if (!IsAmbienceSoundPlaying(ambientSound) && VolumeOfAmbientSound(ambientSound) > 0.0001f)
			{
				SoundInfo info = SoundInfo.OnCamera();
				Sustainer sustainer = ambientSound.TrySpawnSustainer(info);
				if (sustainer != null)
				{
					ambienceSustainers[sustainer.def] = sustainer;
				}
			}
		}
	}

	private bool IsAmbienceSoundPlaying(SoundDef sound)
	{
		return ambienceSustainers.ContainsKey(sound);
	}

	public void WeatherManagerUpdate()
	{
		SetAmbienceSustainersVolume();
	}

	public void EndAllSustainers()
	{
		foreach (KeyValuePair<SoundDef, Sustainer> ambienceSustainer in ambienceSustainers)
		{
			ambienceSustainer.Deconstruct(out var _, out var value);
			value.End();
		}
		ambienceSustainers.Clear();
	}

	public void ResetSkyTargetLerpCache()
	{
		prevSkyTargetLerp = -1f;
		currSkyTargetLerp = -1f;
	}

	private void SetAmbienceSustainersVolume()
	{
		TmpToRemove.Clear();
		foreach (KeyValuePair<SoundDef, Sustainer> ambienceSustainer in ambienceSustainers)
		{
			ambienceSustainer.Deconstruct(out var key, out var value);
			SoundDef soundDef = key;
			Sustainer sustainer = value;
			float num = VolumeOfAmbientSound(soundDef);
			if (num > 0.0001f)
			{
				sustainer.externalParams["LerpFactor"] = num;
				continue;
			}
			sustainer.End();
			TmpToRemove.Add(soundDef);
		}
		foreach (SoundDef item in TmpToRemove)
		{
			ambienceSustainers.Remove(item);
		}
		TmpToRemove.Clear();
	}

	private float VolumeOfAmbientSound(SoundDef soundDef)
	{
		if (map != Find.CurrentMap)
		{
			return 0f;
		}
		for (int i = 0; i < Find.WindowStack.Count; i++)
		{
			if (Find.WindowStack[i].silenceAmbientSound)
			{
				return 0f;
			}
		}
		float num = 0f;
		foreach (SoundDef ambientSound in lastWeather.ambientSounds)
		{
			if (ambientSound == soundDef)
			{
				num += 1f - TransitionLerpFactor;
			}
		}
		foreach (SoundDef ambientSound2 in curWeather.ambientSounds)
		{
			if (ambientSound2 == soundDef)
			{
				num += TransitionLerpFactor;
			}
		}
		return num;
	}

	public void DrawAllWeather()
	{
		eventHandler.WeatherEventsDraw();
		lastWeather.Worker.DrawWeather(map);
		curWeather.Worker.DrawWeather(map);
	}
}
