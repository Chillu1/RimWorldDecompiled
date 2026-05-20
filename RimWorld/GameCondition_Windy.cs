using Verse;

namespace RimWorld;

public class GameCondition_Windy : GameCondition
{
	public override float MinWindSpeed()
	{
		return 1.5f;
	}

	public override float WeatherCommonalityFactor(WeatherDef weather, Map map)
	{
		if (weather == WeatherDefOf.Fog || weather == WeatherDefOf.FoggyRain)
		{
			return 0f;
		}
		return 1f;
	}
}
