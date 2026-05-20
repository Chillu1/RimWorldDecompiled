using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class AnimalPenEnclosureStateCalculator : AnimalPenEnclosureCalculator
	{
		private bool enclosed;

		private bool indirectlyConnected;

		private List<Building_Door> passableDoors = new List<Building_Door>();

		private List<Building_Door> impassableDoors = new List<Building_Door>();

		private List<Region> directlyConnectedRegions = new List<Region>();

		private HashSet<Region> connectedRegions = new HashSet<Region>();

		public bool Enclosed => enclosed;

		public bool IndirectlyConnected => indirectlyConnected;

		public bool PassableDoors => passableDoors.Any();

		public bool ImpassableDoors => impassableDoors.Any();

		public List<Region> DirectlyConnectedRegions => directlyConnectedRegions;

		public HashSet<Region> ConnectedRegions => connectedRegions;

		public bool ContainsConnectedRegion(Region r)
		{
			return connectedRegions.Contains(r);
		}

		public bool NeedsRecalculation()
		{
			foreach (Building_Door passableDoor in passableDoors)
			{
				if (!AnimalPenEnclosureCalculator.RoamerCanPass(passableDoor))
				{
					return true;
				}
			}
			foreach (Building_Door impassableDoor in impassableDoors)
			{
				if (AnimalPenEnclosureCalculator.RoamerCanPass(impassableDoor))
				{
					return true;
				}
			}
			return false;
		}

		public void Recalulate(IntVec3 position, Map map)
		{
			indirectlyConnected = false;
			passableDoors.Clear();
			impassableDoors.Clear();
			connectedRegions.Clear();
			directlyConnectedRegions.Clear();
			enclosed = VisitPen(position, map);
		}

		protected override void VisitDirectlyConnectedRegion(Region r)
		{
			connectedRegions.Add(r);
			directlyConnectedRegions.Add(r);
		}

		protected override void VisitIndirectlyDirectlyConnectedRegion(Region r)
		{
			indirectlyConnected = true;
			connectedRegions.Add(r);
		}

		protected override void VisitPassableDoorway(Region r)
		{
			connectedRegions.Add(r);
			passableDoors.Add(r.door);
		}

		protected override void VisitImpassableDoorway(Region r)
		{
			impassableDoors.Add(r.door);
		}
	}
}
