using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse
{
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
				overlays[i].TickOverlay(map);
			}
			for (int j = 0; j < def.eventMakers.Count; j++)
			{
				def.eventMakers[j].WeatherEventMakerTick(map, lerpFactor);
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
			float t = ((num4 != 0f) ? ((num - skyThreshold.celGlowThreshold) / num4) : 1f);
			SkyTarget result = default(SkyTarget);
			result.glow = num;
			result.colors = SkyColorSet.Lerp(skyThreshold.colors, skyThreshold2.colors, t);
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
	}
}
