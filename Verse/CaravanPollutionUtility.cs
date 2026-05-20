using System.Collections.Generic;
using RimWorld.Planet;

namespace Verse;

public static class CaravanPollutionUtility
{
	private const float ModeratePollutionToxicDamageFactor = 0.5f;

	public static void CheckDamageFromPollution(Caravan caravan, int delta)
	{
		if (caravan.IsHashIntervalTick(3451, delta) && Find.WorldGrid[caravan.Tile].PollutionLevel() >= PollutionLevel.Moderate)
		{
			float extraFactor = ToxicDamagePollutionFactor(caravan.Tile);
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				ToxicUtility.DoPawnToxicDamage(pawnsListForReading[i], extraFactor);
			}
		}
	}

	public static float ToxicDamagePollutionFactor(PlanetTile tile)
	{
		if (Find.WorldGrid[tile].PollutionLevel() == PollutionLevel.Moderate)
		{
			return 0.5f;
		}
		return 1f;
	}
}
