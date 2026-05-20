using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class PenMarkerState
	{
		private readonly CompAnimalPenMarker marker;

		private AnimalPenEnclosureStateCalculator state;

		public bool Enclosed => Calc().Enclosed;

		public bool Unenclosed => !Enclosed;

		public bool PassableDoors => Calc().PassableDoors;

		public bool HasOutsideAccess
		{
			get
			{
				if (Enclosed)
				{
					return Calc().ImpassableDoors;
				}
				return true;
			}
		}

		public List<Region> DirectlyConnectedRegions => Calc().DirectlyConnectedRegions;

		public HashSet<Region> ConnectedRegions => Calc().ConnectedRegions;

		public bool ContainsConnectedRegion(Region r)
		{
			return Calc().ContainsConnectedRegion(r);
		}

		public PenMarkerState(CompAnimalPenMarker marker)
		{
			this.marker = marker;
		}

		private AnimalPenEnclosureStateCalculator Calc()
		{
			if (state == null)
			{
				state = new AnimalPenEnclosureStateCalculator();
				state.Recalulate(marker.parent.Position, marker.parent.Map);
			}
			else if (state.NeedsRecalculation())
			{
				state.Recalulate(marker.parent.Position, marker.parent.Map);
			}
			return state;
		}
	}
}
