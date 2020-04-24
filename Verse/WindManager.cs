using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse.Noise;

namespace Verse
{
	public class WindManager
	{
		private static readonly FloatRange WindSpeedRange = new FloatRange(0.04f, 2f);

		private Map map;

		private static List<Material> plantMaterials = new List<Material>();

		private float cachedWindSpeed;

		private ModuleBase windNoise;

		private float plantSwayHead;

		public float WindSpeed => cachedWindSpeed;

		public WindManager(Map map)
		{
			this.map = map;
		}

		public void WindManagerTick()
		{
			cachedWindSpeed = BaseWindSpeedAt(Find.TickManager.TicksAbs) * map.weatherManager.CurWindSpeedFactor;
			float curWindSpeedOffset = map.weatherManager.CurWindSpeedOffset;
			if (curWindSpeedOffset > 0f)
			{
				FloatRange floatRange = WindSpeedRange * map.weatherManager.CurWindSpeedFactor;
				float num = (cachedWindSpeed - floatRange.min) / (floatRange.max - floatRange.min) * (floatRange.max - curWindSpeedOffset);
				cachedWindSpeed = curWindSpeedOffset + num;
			}
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.WindSource);
			for (int i = 0; i < list.Count; i++)
			{
				CompWindSource compWindSource = list[i].TryGetComp<CompWindSource>();
				cachedWindSpeed = Mathf.Max(cachedWindSpeed, compWindSource.wind);
			}
			if (Prefs.PlantWindSway)
			{
				plantSwayHead += Mathf.Min(WindSpeed, 1f);
			}
			else
			{
				plantSwayHead = 0f;
			}
			if (Find.CurrentMap == map)
			{
				for (int j = 0; j < plantMaterials.Count; j++)
				{
					plantMaterials[j].SetFloat(ShaderPropertyIDs.SwayHead, plantSwayHead);
				}
			}
		}

		public static void Notify_PlantMaterialCreated(Material newMat)
		{
			plantMaterials.Add(newMat);
		}

		private float BaseWindSpeedAt(int ticksAbs)
		{
			if (windNoise == null)
			{
				int seed = Gen.HashCombineInt(map.Tile, 122049541) ^ Find.World.info.Seed;
				windNoise = new Perlin(3.9999998989515007E-05, 2.0, 0.5, 4, seed, QualityMode.Medium);
				windNoise = new ScaleBias(1.5, 0.5, windNoise);
				windNoise = new Clamp(WindSpeedRange.min, WindSpeedRange.max, windNoise);
			}
			return (float)windNoise.GetValue(ticksAbs, 0.0, 0.0);
		}

		public string DebugString()
		{
			return "WindSpeed: " + WindSpeed + "\nplantSwayHead: " + plantSwayHead;
		}

		public void LogWindSpeeds()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Upcoming wind speeds:");
			for (int i = 0; i < 72; i++)
			{
				stringBuilder.AppendLine("Hour " + i + " - " + BaseWindSpeedAt(Find.TickManager.TicksAbs + 2500 * i).ToString("F2"));
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
