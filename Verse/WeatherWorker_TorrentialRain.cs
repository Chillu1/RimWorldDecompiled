using System.Linq;
using RimWorld;

namespace Verse;

public class WeatherWorker_TorrentialRain : WeatherWorker
{
	public WeatherWorker_TorrentialRain(WeatherDef def)
		: base(def)
	{
	}

	public override void OnWeatherStart(Map map)
	{
		base.OnWeatherStart(map);
		GenSpawn.Spawn(ThingDefOf.TorrentialRainFlood, map.Center, map);
	}

	public override void OnWeatherEnd(Map map)
	{
		base.OnWeatherEnd(map);
		map.listerThings.ThingsOfDef(ThingDefOf.TorrentialRainFlood).FirstOrDefault()?.Destroy();
	}
}
