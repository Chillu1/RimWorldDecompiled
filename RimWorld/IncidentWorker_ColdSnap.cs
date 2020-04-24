using Verse;

namespace RimWorld
{
	public class IncidentWorker_ColdSnap : IncidentWorker_MakeGameCondition
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
			if (map.mapTemperature.SeasonalTemp > 0f)
			{
				return map.mapTemperature.SeasonalTemp < 15f;
			}
			return false;
		}
	}
}
