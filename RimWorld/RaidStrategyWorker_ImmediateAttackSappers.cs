using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RaidStrategyWorker_ImmediateAttackSappers : RaidStrategyWorker_WithRequiredPawnKinds
{
	protected override bool MatchesRequiredPawnKind(PawnKindDef kind)
	{
		return kind.canBeSapper;
	}

	protected override int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
	{
		return 1;
	}

	public override bool CanUsePawn(float pointsTotal, Pawn p, List<Pawn> otherPawns)
	{
		if (otherPawns.Count == 0 && !SappersUtility.IsGoodSapper(p) && !SappersUtility.IsGoodBackupSapper(p))
		{
			return false;
		}
		if (p.kindDef.canBeSapper && SappersUtility.HasBuildingDestroyerWeapon(p) && !SappersUtility.IsGoodSapper(p))
		{
			return false;
		}
		return true;
	}

	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		return new LordJob_AssaultColony(parms.faction, canTimeoutOrFlee: parms.canTimeoutOrFlee, canKidnap: parms.canKidnap, sappers: true, useAvoidGridSmart: true, canSteal: parms.canSteal);
	}
}
