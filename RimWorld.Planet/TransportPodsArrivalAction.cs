using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public abstract class TransportPodsArrivalAction : IExposable
	{
		public virtual FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			return true;
		}

		public virtual bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return false;
		}

		public abstract void Arrived(List<ActiveDropPodInfo> pods, int tile);

		public virtual void ExposeData()
		{
		}
	}
}
