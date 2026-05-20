using System.Linq;

namespace RimWorld;

public class HistoryAutoRecorderWorker_FreeColonists : HistoryAutoRecorderWorker
{
	public override float PullRecord()
	{
		return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers.Count();
	}
}
