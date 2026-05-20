using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_GorehulkAssault : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.faction = Faction.OfEntities;
		parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
		PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Gorehulks, parms);
		float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Gorehulks);
		if (parms.points < num)
		{
			parms.points = (defaultPawnGroupMakerParms.points = num * 2f);
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
		if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
		{
			return false;
		}
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		parms.raidArrivalMode.Worker.Arrive(list, parms);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_GorehulkAssault(), parms.target as Map, list);
		SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, list);
		return true;
	}
}
