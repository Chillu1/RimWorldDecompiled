using Verse;

namespace RimWorld;

public class IncidentWorker_OrbitalDebris : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (ModsConfig.OdysseyActive)
		{
			return base.CanFireNowSub(parms);
		}
		return false;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		Map map = (Map)parms.target;
		GenSpawn.Spawn(ThingDefOf.OrbitalDebrisSpawner, map.Center, map);
		SendStandardLetter(parms, null);
		return true;
	}
}
