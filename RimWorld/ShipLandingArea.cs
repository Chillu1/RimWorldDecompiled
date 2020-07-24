using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ShipLandingArea
	{
		private CellRect rect;

		private Map map;

		private Thing firstBlockingThing;

		private bool blockedByRoof;

		public List<CompShipLandingBeacon> beacons = new List<CompShipLandingBeacon>();

		public IntVec3 CenterCell => rect.CenterCell;

		public CellRect MyRect => rect;

		public bool Clear
		{
			get
			{
				if (firstBlockingThing == null)
				{
					return !blockedByRoof;
				}
				return false;
			}
		}

		public bool BlockedByRoof => blockedByRoof;

		public Thing FirstBlockingThing => firstBlockingThing;

		public bool Active
		{
			get
			{
				for (int i = 0; i < beacons.Count; i++)
				{
					if (!beacons[i].Active)
					{
						return false;
					}
				}
				return true;
			}
		}

		public ShipLandingArea(CellRect rect, Map map)
		{
			this.rect = rect;
			this.map = map;
		}

		public void RecalculateBlockingThing()
		{
			blockedByRoof = false;
			foreach (IntVec3 item in rect)
			{
				if (item.Roofed(map))
				{
					blockedByRoof = true;
					break;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (!(thingList[i] is Pawn) && (thingList[i].def.Fillage != 0 || thingList[i].def.IsEdifice() || thingList[i] is Skyfaller))
					{
						firstBlockingThing = thingList[i];
						return;
					}
				}
			}
			firstBlockingThing = null;
		}
	}
}
