using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttackSmart : RaidStrategyWorker
	{
		protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
		{
			return new LordJob_AssaultColony(parms.faction, canKidnap: true, canTimeoutOrFlee: true, sappers: false, useAvoidGridSmart: true);
		}

		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!base.CanUseWith(parms, groupKind))
			{
				return false;
			}
			if (!parms.faction.def.canUseAvoidGrid)
			{
				return false;
			}
			return true;
		}
	}
}
