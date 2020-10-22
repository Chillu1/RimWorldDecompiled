using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttack : RaidStrategyWorker
	{
		protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
		{
			IntVec3 originCell = (parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld);
			if (parms.faction.HostileTo(Faction.OfPlayer))
			{
				return new LordJob_AssaultColony(parms.faction);
			}
			RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, out var result);
			return new LordJob_AssistColony(parms.faction, result);
		}
	}
}
