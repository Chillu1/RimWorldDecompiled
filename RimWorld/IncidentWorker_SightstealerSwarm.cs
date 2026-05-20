using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class IncidentWorker_SightstealerSwarm : IncidentWorker
{
	private const float PointsFactor = 0.5f;

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		Map map = (Map)parms.target;
		List<Pawn> list = GenerateSightstealers(parms, parms.points * 0.5f);
		PawnsArrivalModeDefOf.EdgeWalkInDistributed.Worker.TryResolveRaidSpawnCenter(parms);
		PawnsArrivalModeDefOf.EdgeWalkInDistributed.Worker.Arrive(list, parms);
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_SightstealerAssault(), map, list);
		SoundDefOf.Sightstealer_DistantHowl.PlayOneShotOnCamera();
		SendStandardLetter(parms, list);
		return true;
	}

	private List<Pawn> GenerateSightstealers(IncidentParms parms, float points)
	{
		Map map = (Map)parms.target;
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Sightstealers,
			tile = map.Tile,
			faction = Faction.OfEntities,
			points = points
		};
		pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind) * 1.05f);
		return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
	}
}
