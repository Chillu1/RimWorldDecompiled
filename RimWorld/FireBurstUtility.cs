using Verse;

namespace RimWorld;

public static class FireBurstUtility
{
	private const float FuelSpawnChancePerCastingTick = 0.15f;

	public static void ThrowFuelTick(IntVec3 position, float radius, Map map)
	{
		if (!Rand.Chance(0.15f))
		{
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(position, radius, useCenter: true).InRandomOrder())
		{
			if (GenSight.LineOfSight(position, item, map, skipFirstCell: true) && FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Fuel))
			{
				break;
			}
		}
	}
}
