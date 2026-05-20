using Verse;

namespace RimWorld;

public class IncidentWorker_LavaFlow : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		((Map)parms.target).GameConditionManager.RegisterCondition(GameConditionMaker.MakeConditionPermanent(GameConditionDefOf.LavaFlow));
		return true;
	}
}
