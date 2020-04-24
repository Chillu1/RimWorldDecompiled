using Verse;

namespace RimWorld
{
	public class IncidentWorker_HeatWave : IncidentWorker_MakeGameCondition
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			return IsTemperatureAppropriate((Map)parms.target);
		}

		public static bool IsTemperatureAppropriate(Map map)
		{
			return map.mapTemperature.SeasonalTemp >= 20f;
		}
	}
}
