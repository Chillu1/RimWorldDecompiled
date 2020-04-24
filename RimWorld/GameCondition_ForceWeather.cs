using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class GameCondition_ForceWeather : GameCondition
	{
		public WeatherDef weather;

		public override void Init()
		{
			base.Init();
			if (weather == null)
			{
				weather = def.weatherDef;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref weather, "weather");
		}

		public override WeatherDef ForcedWeather()
		{
			return weather;
		}

		public override void RandomizeSettings(float points, Map map, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.RandomizeSettings(points, map, outExtraDescriptionRules, outExtraDescriptionConstants);
			weather = DefDatabase<WeatherDef>.AllDefsListForReading.Where((WeatherDef def) => def.isBad).RandomElement();
			outExtraDescriptionRules.AddRange(GrammarUtility.RulesForDef("forcedWeather", weather));
		}
	}
}
