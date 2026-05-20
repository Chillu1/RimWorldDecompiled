using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_WastepackInfestation : IncidentWorker
{
	public override float BaseChanceThisGame => base.BaseChanceThisGame * Find.Storyteller.difficulty.wastepackInfestationChanceFactor;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (base.CanFireNowSub(parms) && Faction.OfInsects != null && GetSpawnCenter(parms, map).IsValid)
		{
			return CocoonInfestationUtility.GetCocoonsToSpawn(parms.points).Any();
		}
		return false;
	}

	private IntVec3 GetSpawnCenter(IncidentParms parms, Map map)
	{
		if (CocoonInfestationUtility.CanSpawnCocoonAt(parms.spawnCenter, map))
		{
			return parms.spawnCenter;
		}
		if (RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, (IntVec3 c) => CocoonInfestationUtility.CanSpawnCocoonAt(c, map), map, out var result))
		{
			return result;
		}
		return IntVec3.Invalid;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		List<Thing> targets = CocoonInfestationUtility.SpawnCocoonInfestation(GetSpawnCenter(parms, map), (Map)parms.target, parms.points);
		SendStandardLetter(parms, new LookTargets(targets));
		Find.TickManager.slower.SignalForceNormalSpeedShort();
		return true;
	}
}
