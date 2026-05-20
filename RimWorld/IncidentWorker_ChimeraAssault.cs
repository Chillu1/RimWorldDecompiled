using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_ChimeraAssault : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.faction = Faction.OfEntities;
		parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
		PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Chimeras, parms);
		float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Chimeras);
		if (parms.points < num)
		{
			parms.points = (defaultPawnGroupMakerParms.points = num * 2f);
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
		if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
		{
			return false;
		}
		parms.raidArrivalMode.Worker.Arrive(list, parms);
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_ChimeraAssault(), parms.target as Map, list);
		SendStandardLetter("ChimeraAssaultLabel".Translate(), "ChimeraAssaultText".Translate(), LetterDefOf.ThreatSmall, parms, list);
		return true;
	}
}
