using Verse;

namespace RimWorld;

public class CompProperties_Shuttle : CompProperties
{
	public TransportShipDef shipDef;

	public CompProperties_Shuttle()
	{
		compClass = typeof(CompShuttle);
	}
}
