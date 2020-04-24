using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public sealed class WeatherManager : IExposable
	{
		public Map map;

		public WeatherEventHandler eventHandler = new WeatherEventHandler();

		public WeatherDef curWeather = WeatherDefOf.Clear;

		public WeatherDef lastWeather = WeatherDefOf.Clear;

		public int curWeatherAge;

		private List<Sustainer> ambienceSustainers = new List<Sustainer>();

		public TemperatureMemory growthSeasonMemory;

		public const float TransitionTicks = 4000f;

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

		public float CurWindSpeedFactor => Mathf.Lerp(lastWeather.windSpeedFactor, curWeather.windSpeedFactor, TransitionLerpFactor);

		public float CurWindSpeedOffset => Mathf.Lerp(lastWeather.windSpeedOffset, curWeather.windSpeedOffset, TransitionLerpFactor);

		public float CurMoveSpeedMultiplier => Mathf.Lerp(lastWeather.moveSpeedMultiplier, curWeather.moveSpeedMultiplier, TransitionLerpFactor);

		public float CurWeatherAccuracyMultiplier => Mathf.Lerp(lastWeather.accuracyMultiplier, curWeather.accuracyMultiplier, TransitionLerpFactor);

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
				float num = 0f;
				num = ((curWeather.perceivePriority > lastWeather.perceivePriority) ? 0.18f : ((!(lastWeather.perceivePriority > curWeather.perceivePriority)) ? 0.5f : 0.82f));
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
			growthSeasonMemory = new TemperatureMemory(map);
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref curWeather, "curWeather");
			Scribe_Defs.Look(ref lastWeather, "lastWeather");
			Scribe_Values.Look(ref curWeatherAge, "curWeatherAge", 0, forceSave: true);
			Scribe_Deep.Look(ref growthSeasonMemory, "growthSeasonMemory", map);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				ambienceSustainers.Clear();
			}
		}

		public void TransitionTo(WeatherDef newWeather)
		{
			lastWeather = curWeather;
			curWeather = newWeather;
			curWeatherAge = 0;
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
			lastWeather.Worker.WeatherTick(map, 1f - TransitionLerpFactor);
			growthSeasonMemory.GrowthSeasonMemoryTick();
			for (int i = 0; i < curWeather.ambientSounds.Count; i++)
			{
				bool flag = false;
				for (int num = ambienceSustainers.Count - 1; num >= 0; num--)
				{
					if (ambienceSustainers[num].def == curWeather.ambientSounds[i])
					{
						flag = true;
						break;
					}
				}
				if (!flag && VolumeOfAmbientSound(curWeather.ambientSounds[i]) > 0.0001f)
				{
					SoundInfo info = SoundInfo.OnCamera();
					Sustainer sustainer = curWeather.ambientSounds[i].TrySpawnSustainer(info);
					if (sustainer != null)
					{
						ambienceSustainers.Add(sustainer);
					}
				}
			}
		}

		public void WeatherManagerUpdate()
		{
			SetAmbienceSustainersVolume();
		}

		public void EndAllSustainers()
		{
			for (int i = 0; i < ambienceSustainers.Count; i++)
			{
				ambienceSustainers[i].End();
			}
			ambienceSustainers.Clear();
		}

		private void SetAmbienceSustainersVolume()
		{
			for (int num = ambienceSustainers.Count - 1; num >= 0; num--)
			{
				float num2 = VolumeOfAmbientSound(ambienceSustainers[num].def);
				if (num2 > 0.0001f)
				{
					ambienceSustainers[num].externalParams["LerpFactor"] = num2;
				}
				else
				{
					ambienceSustainers[num].End();
					ambienceSustainers.RemoveAt(num);
				}
			}
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
			for (int j = 0; j < lastWeather.ambientSounds.Count; j++)
			{
				if (lastWeather.ambientSounds[j] == soundDef)
				{
					num += 1f - TransitionLerpFactor;
				}
			}
			for (int k = 0; k < curWeather.ambientSounds.Count; k++)
			{
				if (curWeather.ambientSounds[k] == soundDef)
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
}
