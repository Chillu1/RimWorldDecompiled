using Verse;

namespace RimWorld;

public class CompProperties_OxygenPusher : CompProperties
{
	public bool requiresPower = true;

	public float airPerSecondPerHundredCells = 0.1f;

	public CompProperties_OxygenPusher()
	{
		compClass = typeof(CompOxygenPusher);
	}
}
