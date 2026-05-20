using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_MechCluster : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.RoyaltyActive)
		{
			return false;
		}
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		return Faction.OfMechanoids != null;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		MechClusterSketch sketch = MechClusterGenerator.GenerateClusterSketch(parms.points, map);
		IntVec3 center = MechClusterUtility.FindClusterPosition(map, sketch, 100, 0.5f);
		if (!center.IsValid)
		{
			return false;
		}
		IEnumerable<Thing> targets = from t in MechClusterUtility.SpawnCluster(center, map, sketch, dropInPods: true, canAssaultColony: true, parms.questTag)
			where t.def != ThingDefOf.Wall && t.def != ThingDefOf.Barricade
			select t;
		SendStandardLetter(parms, new LookTargets(targets));
		return true;
	}
}
