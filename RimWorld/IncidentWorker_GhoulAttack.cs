using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_GhoulAttack : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Hostile))
		{
			return false;
		}
		Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Ghoul, Faction.OfEntities);
		if (pawn == null)
		{
			return false;
		}
		pawn.health.overrideDeathOnDownedChance = 0f;
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(pawn);
		}
		Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
		GenSpawn.Spawn(pawn, result, map, rot);
		IncidentWorker.SendIncidentLetter(def.letterLabel, def.letterText, LetterDefOf.ThreatSmall, parms, pawn, def);
		Find.TickManager.slower.SignalForceNormalSpeedShort();
		LordMaker.MakeNewLord(parms.faction, new LordJob_AssaultColony(Faction.OfEntities, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), map, Gen.YieldSingle(pawn));
		return true;
	}
}
