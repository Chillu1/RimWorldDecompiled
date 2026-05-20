using Verse;

namespace RimWorld.Planet;

public class PlanetLayerConnection : IExposable
{
	public PlanetLayer origin;

	public PlanetLayer target;

	public float fuelCost;

	public void ExposeData()
	{
		Scribe_Values.Look(ref fuelCost, "fuelCost", 0f);
		Scribe_References.Look(ref origin, "origin");
		Scribe_References.Look(ref target, "target");
	}
}
