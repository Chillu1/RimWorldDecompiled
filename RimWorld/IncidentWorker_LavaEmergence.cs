using Verse;

namespace RimWorld;

public class IncidentWorker_LavaEmergence : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		GenSpawn.Spawn(ThingDefOf.LavaEmergence, map.Center, map);
		return true;
	}
}
