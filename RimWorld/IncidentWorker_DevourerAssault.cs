using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_DevourerAssault : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.faction = Faction.OfEntities;
		parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
		PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Devourers, parms);
		float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Devourers);
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
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_DevourerAssault(), parms.target as Map, list);
		SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, list);
		return true;
	}
}
