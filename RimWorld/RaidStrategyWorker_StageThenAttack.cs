using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RaidStrategyWorker_StageThenAttack : RaidStrategyWorker
{
	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		IntVec3 stageLoc = RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld, map, allowRoofed: true, errorOnFail: false);
		if (!stageLoc.IsValid)
		{
			stageLoc = pawns[0].PositionHeld;
		}
		return new LordJob_StageThenAttack(parms.faction, stageLoc, raidSeed);
	}

	public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		if (!base.CanUseWith(parms, groupKind))
		{
			return false;
		}
		return parms.faction.def.canStageAttacks;
	}

	public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g, List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
	{
		if (g.kind.RaceProps.Animal)
		{
			return false;
		}
		return base.CanUsePawnGenOption(pointsTotal, g, chosenGroups, faction);
	}
}
