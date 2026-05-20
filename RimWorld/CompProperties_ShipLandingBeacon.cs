using Verse;

namespace RimWorld;

public class CompProperties_ShipLandingBeacon : CompProperties
{
	public FloatRange edgeLengthRange;

	public TerrainAffordanceDef landingAreaTerrainSupport;

	public CompProperties_ShipLandingBeacon()
	{
		compClass = typeof(CompShipLandingBeacon);
	}
}
