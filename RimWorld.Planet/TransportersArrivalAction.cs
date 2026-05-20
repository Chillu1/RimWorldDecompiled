using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public abstract class TransportersArrivalAction : IExposable
{
	public abstract bool GeneratesMap { get; }

	public virtual FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
	{
		return true;
	}

	public virtual bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods, PlanetTile tile)
	{
		return false;
	}

	public abstract void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile);

	public virtual void ExposeData()
	{
	}
}
