using Verse;

namespace RimWorld;

public class IncidentWorker_CreepJoiner_Metalhorror : IncidentWorker_GiveQuest
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		return Find.Anomaly.CanNewMetalhorrorBiosignatureImplantOccur;
	}
}
