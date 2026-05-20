using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_DevourerWaterAssault : IncidentWorker
{
	private const float PointsMultiplier = 0.7f;

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.faction = Faction.OfEntities;
		parms.raidArrivalMode = PawnsArrivalModeDefOf.EmergeFromWater;
		PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Devourers, parms);
		float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Devourers);
		defaultPawnGroupMakerParms.points = parms.points * 0.7f;
		if (defaultPawnGroupMakerParms.points < num)
		{
			defaultPawnGroupMakerParms.points = num * 2f;
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
		if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
		{
			return false;
		}
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_DevourerAssault(), parms.target as Map);
		parms.lord = lord;
		parms.raidArrivalMode.Worker.Arrive(list, parms);
		if (AnomalyIncidentUtility.IncidentShardChance(defaultPawnGroupMakerParms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, list);
		return true;
	}
}
