using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class TransportShipMaker
	{
		public static TransportShip MakeTransportShip(TransportShipDef def, IEnumerable<Thing> contents, Thing shipThing = null)
		{
			TransportShip transportShip = new TransportShip(def);
			transportShip.shipThing = shipThing ?? ThingMaker.MakeThing(def.shipThing);
			CompShuttle compShuttle = transportShip.shipThing.TryGetComp<CompShuttle>();
			if (compShuttle != null)
			{
				compShuttle.shipParent = transportShip;
			}
			if (contents != null)
			{
				transportShip.TransporterComp.innerContainer.TryAddRangeOrTransfer(contents, canMergeWithExistingStacks: true, destroyLeftover: true);
			}
			return transportShip;
		}
	}
}
