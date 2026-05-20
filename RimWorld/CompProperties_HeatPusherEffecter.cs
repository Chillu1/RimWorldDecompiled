using Verse;

namespace RimWorld;

public class CompProperties_HeatPusherEffecter : CompProperties
{
	public EffecterDef effecterDef;

	public CompProperties_HeatPusherEffecter()
	{
		compClass = typeof(CompHeatPusherEffecter);
	}
}
