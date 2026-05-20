using Verse;

namespace RimWorld;

public class CompProperties_LowPowerUnlessVacuum : CompProperties
{
	public float lowPowerConsumptionFactor = 0.1f;

	public bool checkRoomVacuum = true;

	public CompProperties_LowPowerUnlessVacuum()
	{
		compClass = typeof(CompLowPowerInSpace);
	}
}
