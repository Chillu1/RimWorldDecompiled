using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class WeatherDecider : IExposable
{
	private Map map;

	private int curWeatherDuration = 10000;

	private int ticksWhenRainAllowedAgain;

	private const int FirstWeatherDuration = 10000;

	private const float ChanceFactorRainOnFire = 15f;

	private static List<GameCondition> allConditionsTmp = new List<GameCondition>();

	public WeatherDef ForcedWeather
	{
		get
		{
			allConditionsTmp.Clear();
			map.gameConditionManager.GetAllGameConditionsAffectingMap(map, allConditionsTmp);
			WeatherDef result = null;
			foreach (GameCondition item in allConditionsTmp)
			{
				WeatherDef weatherDef = item.ForcedWeather();
				if (weatherDef != null)
				{
					result = weatherDef;
				}
			}
			return result;
		}
	}

	public IEnumerable<WeatherCommonalityRecord> WeatherCommonalities => map.Biome.baseWeatherCommonalities;

	public bool ClearWeatherAllowed
	{
		get
		{
			if (!WeatherCommonalities.EnumerableNullOrEmpty())
			{
				return WeatherCommonalities.Any((WeatherCommonalityRecord w) => w.weather == WeatherDefOf.Clear);
			}
			return false;
		}
	}

	public WeatherDecider(Map map)
	{
		this.map = map;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref curWeatherDuration, "curWeatherDuration", 0, forceSave: true);
		Scribe_Values.Look(ref ticksWhenRainAllowedAgain, "ticksWhenRainAllowedAgain", 0);
	}

	public void WeatherDeciderTick()
	{
		WeatherDef forcedWeather = ForcedWeather;
		int num = curWeatherDuration;
		if (map.fireWatcher.LargeFireDangerPresent || !map.weatherManager.curWeather.temperatureRange.Includes(map.mapTemperature.OutdoorTemp) || (ModsConfig.AnomalyActive && map.gameConditionManager.BrightnessChanging))
		{
			num = (int)((float)num * 0.25f);
		}
		if (forcedWeather != null && map.weatherManager.curWeather != forcedWeather)
		{
			num = ((!ModsConfig.AnomalyActive || forcedWeather.transitionTicksOverride == int.MaxValue) ? 4000 : forcedWeather.transitionTicksOverride);
		}
		if (map.weatherManager.curWeatherAge > num)
		{
			StartNextWeather();
		}
	}

	public void StartNextWeather()
	{
		WeatherDef weatherDef = ChooseNextWeather();
		map.weatherManager.TransitionTo(weatherDef);
		curWeatherDuration = weatherDef.durationRange.RandomInRange;
	}

	public void StartInitialWeather()
	{
		if (Find.GameInitData != null)
		{
			if (ClearWeatherAllowed)
			{
				map.weatherManager.curWeather = WeatherDefOf.Clear;
			}
			else
			{
				map.weatherManager.curWeather = WeatherCommonalities.RandomElement().weather;
			}
			curWeatherDuration = 10000;
			map.weatherManager.lastWeather = map.weatherManager.curWeather;
			map.weatherManager.curWeatherAge = 0;
		}
		else
		{
			map.weatherManager.curWeather = null;
			WeatherDef weatherDef = ChooseNextWeather();
			WeatherDef lastWeather = ChooseNextWeather();
			map.weatherManager.curWeather = weatherDef;
			map.weatherManager.lastWeather = lastWeather;
			curWeatherDuration = weatherDef.durationRange.RandomInRange;
			map.weatherManager.curWeatherAge = Rand.Range(0, curWeatherDuration);
		}
		map.weatherManager.ResetSkyTargetLerpCache();
	}

	private WeatherDef ChooseNextWeather()
	{
		if (TutorSystem.TutorialMode && ClearWeatherAllowed)
		{
			return WeatherDefOf.Clear;
		}
		WeatherDef forcedWeather = ForcedWeather;
		if (forcedWeather != null)
		{
			return forcedWeather;
		}
		if (!DefDatabase<WeatherDef>.AllDefs.TryRandomElementByWeight(CurrentWeatherCommonality, out var result))
		{
			Log.Warning("All weather commonalities were zero. Defaulting to " + WeatherDefOf.Clear.defName + ".");
			if (!WeatherCommonalities.EnumerableNullOrEmpty())
			{
				return WeatherCommonalities.RandomElement().weather;
			}
			return WeatherDefOf.Clear;
		}
		return result;
	}

	public void DisableRainFor(int ticks)
	{
		ticksWhenRainAllowedAgain = Find.TickManager.TicksGame + ticks;
	}

	private float CurrentWeatherCommonality(WeatherDef weather)
	{
		if (map.weatherManager.curWeather != null && !map.weatherManager.curWeather.repeatable && weather == map.weatherManager.curWeather)
		{
			return 0f;
		}
		if (!weather.temperatureRange.Includes(map.mapTemperature.OutdoorTemp))
		{
			return 0f;
		}
		if ((int)weather.favorability < 2 && GenDate.DaysPassedSinceSettle < 8)
		{
			return 0f;
		}
		if (weather.rainRate > 0.1f && Find.TickManager.TicksGame < ticksWhenRainAllowedAgain)
		{
			return 0f;
		}
		if (weather.rainRate > 0.1f && map.gameConditionManager.ActiveConditions.Any((GameCondition x) => x.def.preventRain))
		{
			return 0f;
		}
		if (ModsConfig.AnomalyActive && weather.minMonolithLevel > Find.Anomaly.HighestLevelReached && (!weather.canOccurInAmbientHorror || !Find.Anomaly.AmbientHorrorMode))
		{
			return 0f;
		}
		BiomeDef biome = map.Biome;
		float commonality = 0f;
		for (int num = 0; num < biome.baseWeatherCommonalities.Count; num++)
		{
			WeatherCommonalityRecord weatherCommonalityRecord = biome.baseWeatherCommonalities[num];
			if (weatherCommonalityRecord.weather != weather)
			{
				continue;
			}
			float num2 = weatherCommonalityRecord.commonality;
			if (map.fireWatcher.LargeFireDangerPresent && weather.rainRate > 0.1f)
			{
				num2 *= 15f;
			}
			if (weatherCommonalityRecord.weather.commonalityRainfallFactor != null)
			{
				num2 *= weatherCommonalityRecord.weather.commonalityRainfallFactor.Evaluate(map.TileInfo.rainfall);
			}
			foreach (GameCondition activeCondition in map.gameConditionManager.ActiveConditions)
			{
				num2 *= activeCondition.WeatherCommonalityFactor(weather, map);
			}
			commonality = num2;
			break;
		}
		PlanetTile tile = map.Tile;
		for (int num3 = 0; num3 < map.TileInfo.Mutators.Count; num3++)
		{
			map.TileInfo.Mutators[num3].Worker?.MutateWeatherCommonalityFor(weather, tile, ref commonality);
		}
		return commonality;
	}

	public void LogWeatherChances()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (WeatherDef item in DefDatabase<WeatherDef>.AllDefs.OrderByDescending((WeatherDef w) => CurrentWeatherCommonality(w)))
		{
			stringBuilder.AppendLine(item.label + " - " + CurrentWeatherCommonality(item));
		}
		Log.Message(stringBuilder.ToString());
	}
}
