using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class TransportShipManager : IExposable
{
	private List<TransportShip> ships = new List<TransportShip>();

	public List<TransportShip> AllTransportShips => ships;

	public void RegisterShipObject(TransportShip s)
	{
		ships.Add(s);
	}

	public void DeregisterShipObject(TransportShip s)
	{
		if (s != null)
		{
			s.EndCurrentJob();
			ships.Remove(s);
		}
	}

	public void ShipObjectsTick()
	{
		for (int num = ships.Count - 1; num >= 0; num--)
		{
			ships[num].Tick();
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref ships, "ships", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (ships.RemoveAll((TransportShip x) => x == null) != 0)
			{
				Log.Error("Removed some null ship objects.");
			}
			ships.RemoveAll((TransportShip x) => x.shipThing == null);
		}
	}
}
