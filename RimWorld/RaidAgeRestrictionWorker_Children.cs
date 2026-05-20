using Verse;

namespace RimWorld;

public class RaidAgeRestrictionWorker_Children : RaidAgeRestrictionWorker
{
	public override bool CanUseWith(IncidentParms parms)
	{
		if (!Find.Storyteller.difficulty.ChildRaidersAllowed || Find.Storyteller.difficulty.babiesAreHealthy)
		{
			return false;
		}
		return base.CanUseWith(parms);
	}
}
