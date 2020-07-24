using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_StageThenAttack : RaidStrategyWorker
	{
		protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
		{
			IntVec3 stageLoc = RCellFinder.FindSiegePositionFrom_NewTemp(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld, map);
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
	}
}
