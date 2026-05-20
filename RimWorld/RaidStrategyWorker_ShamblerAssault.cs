using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RaidStrategyWorker_ShamblerAssault : RaidStrategyWorker_ImmediateAttack
{
	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		return new LordJob_ShamblerAssault();
	}
}
