using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_ShortCircuit : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		return ShortCircuitUtility.GetShortCircuitablePowerConduits((Map)parms.target).Any();
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ShortCircuitUtility.GetShortCircuitablePowerConduits((Map)parms.target).TryRandomElement(out var result))
		{
			return false;
		}
		ShortCircuitUtility.DoShortCircuit(result);
		return true;
	}
}
