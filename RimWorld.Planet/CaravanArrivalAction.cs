using Verse;

namespace RimWorld.Planet
{
	public abstract class CaravanArrivalAction : IExposable
	{
		public abstract string Label
		{
			get;
		}

		public abstract string ReportString
		{
			get;
		}

		public virtual FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			return true;
		}

		public abstract void Arrived(Caravan caravan);

		public virtual void ExposeData()
		{
		}
	}
}
