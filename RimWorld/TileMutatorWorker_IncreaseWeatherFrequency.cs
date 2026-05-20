using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_IncreaseWeatherFrequency : TileMutatorWorker
{
	public TileMutatorWorker_IncreaseWeatherFrequency(TileMutatorDef def)
		: base(def)
	{
	}

	public override void MutateWeatherCommonalityFor(WeatherDef weather, PlanetTile tile, ref float commonality)
	{
		if (def.weathersToAffect.Contains(weather))
		{
			commonality += def.weatherFrequencyOffset;
			commonality *= def.weatherFrequencyFactor;
		}
	}
}
