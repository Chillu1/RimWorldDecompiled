using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class IncidentWorker_NeutralGroup : IncidentWorker_PawnsArrive
{
	protected virtual PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Peaceful;

	public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
	{
		if (!base.FactionCanBeGroupSource(f, parms, desperate))
		{
			return false;
		}
		if (f.Hidden || f.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (f.def.pawnGroupMakers == null || !f.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDef))
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f))
		{
			return false;
		}
		if (!f.def.neutralArrivalLayerWhitelist.NullOrEmpty() && !f.def.neutralArrivalLayerWhitelist.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		if (!f.def.neutralArrivalLayerBlacklist.NullOrEmpty() && f.def.neutralArrivalLayerBlacklist.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		return true;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		foreach (GameCondition activeCondition in ((Map)parms.target).GameConditionManager.ActiveConditions)
		{
			if (activeCondition.def.preventNeutralVisitors)
			{
				return false;
			}
		}
		return true;
	}

	protected bool TryResolveParms(IncidentParms parms)
	{
		if (!TryResolveParmsGeneral(parms))
		{
			return false;
		}
		ResolveParmsPoints(parms);
		return true;
	}

	protected virtual bool TryResolveParmsGeneral(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral))
		{
			return false;
		}
		if (parms.faction == null)
		{
			if (CandidateFactions(parms).TryRandomElement(out parms.faction))
			{
				return true;
			}
			return CandidateFactions(parms, desperate: true).TryRandomElement(out parms.faction);
		}
		return true;
	}

	protected abstract void ResolveParmsPoints(IncidentParms parms);

	protected List<Pawn> SpawnPawns(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true), warnOnZeroResults: false).ToList();
		foreach (Pawn item in list)
		{
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
			GenSpawn.Spawn(item, loc, map);
			parms.storeGeneratedNeutralPawns?.Add(item);
		}
		return list;
	}
}
