using System.Collections.Generic;

namespace RimWorld
{
	public class GatherSpotLister
	{
		public List<CompGatherSpot> activeSpots = new List<CompGatherSpot>();

		public void RegisterActivated(CompGatherSpot spot)
		{
			if (!activeSpots.Contains(spot))
			{
				activeSpots.Add(spot);
			}
		}

		public void RegisterDeactivated(CompGatherSpot spot)
		{
			if (activeSpots.Contains(spot))
			{
				activeSpots.Remove(spot);
			}
		}
	}
}
