using System.Linq;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_Prisoners : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony.Count();
		}
	}
}
