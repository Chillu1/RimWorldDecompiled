using System.Linq;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_FreeColonists : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Count();
		}
	}
}
