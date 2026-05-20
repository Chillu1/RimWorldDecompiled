using Verse;

namespace RimWorld;

public class ThoughtWorker_UnnaturalCorpse : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!Find.Anomaly.TryGetUnnaturalCorpseTrackerForHaunted(p, out var _))
		{
			return false;
		}
		return ThoughtState.ActiveDefault;
	}
}
