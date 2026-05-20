using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse.Noise;

namespace Verse;

public class WindManager
{
	private static readonly FloatRange WindSpeedRange = new FloatRange(0.04f, 2f);

	private Map map;

	private static List<Material> plantMaterials = new List<Material>();

	private float cachedWindSpeed;

	private ModuleBase windNoise;

	private float plantSwayHead;

	private List<GameCondition> tempAllGameConditionsAffectingMap = new List<GameCondition>();

	public float WindSpeed => cachedWindSpeed;

	public WindManager(Map map)
	{
		this.map = map;
	}

	public void WindManagerTick()
	{
		cachedWindSpeed = BaseWindSpeedAt(Find.TickManager.TicksAbs) * map.weatherManager.CurWindSpeedFactor;
		float num = map.weatherManager.CurWindSpeedOffset;
		tempAllGameConditionsAffectingMap.Clear();
		map.gameConditionManager.GetAllGameConditionsAffectingMap(map, tempAllGameConditionsAffectingMap);
		for (int i = 0; i < tempAllGameConditionsAffectingMap.Count; i++)
		{
			float num2 = tempAllGameConditionsAffectingMap[i].MinWindSpeed();
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num > 0f)
		{
			FloatRange floatRange = WindSpeedRange * map.weatherManager.CurWindSpeedFactor;
			float num3 = (cachedWindSpeed - floatRange.min) / (floatRange.max - floatRange.min) * (floatRange.max - num);
			cachedWindSpeed = num + num3;
		}
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.WindSource);
		for (int j = 0; j < list.Count; j++)
		{
			CompWindSource compWindSource = list[j].TryGetComp<CompWindSource>();
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
			for (int k = 0; k < plantMaterials.Count; k++)
			{
				plantMaterials[k].SetFloat(ShaderPropertyIDs.SwayHead, plantSwayHead);
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
			int seed = Gen.HashCombineInt(map.GetHashCode(), 122049541) ^ Find.World.info.Seed;
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
