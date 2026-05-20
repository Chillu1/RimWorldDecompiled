using Verse;

namespace RimWorld;

public class IncidentWorker_Aurora : IncidentWorker_MakeGameCondition
{
	private const int EnsureMinDurationTicks = 5000;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.IsPlayerHome && !map.GameConditionManager.IsAlwaysDarkOutside && !AuroraWillEndSoon(map))
			{
				return true;
			}
		}
		return false;
	}

	private bool AuroraWillEndSoon(Map map)
	{
		if (GenCelestial.CurCelestialSunGlow(map) > 0.5f)
		{
			return true;
		}
		if (GenCelestial.CelestialSunGlow(map, Find.TickManager.TicksAbs + 5000) > 0.5f)
		{
			return true;
		}
		return false;
	}
}
