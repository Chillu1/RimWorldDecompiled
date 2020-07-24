using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_Siege : RaidStrategyWorker
	{
		protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
		{
			IntVec3 siegeSpot = RCellFinder.FindSiegePositionFrom_NewTemp(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld, map);
			float num = parms.points * Rand.Range(0.2f, 0.3f);
			if (num < 60f)
			{
				num = 60f;
			}
			return new LordJob_Siege(parms.faction, siegeSpot, num);
		}

		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!base.CanUseWith(parms, groupKind))
			{
				return false;
			}
			return parms.faction.def.canSiege;
		}
	}
}
