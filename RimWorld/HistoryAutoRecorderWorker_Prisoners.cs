using System.Linq;

namespace RimWorld;

public class HistoryAutoRecorderWorker_Prisoners : HistoryAutoRecorderWorker
{
	public override float PullRecord()
	{
		return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony.Count();
	}
}
