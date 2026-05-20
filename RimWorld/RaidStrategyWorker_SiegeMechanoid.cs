using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RaidStrategyWorker_SiegeMechanoid : RaidStrategyWorker_Siege
{
	public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		if (parms.points < MinimumPoints(parms.faction, groupKind))
		{
			return false;
		}
		if (Faction.OfMechanoids == null)
		{
			return false;
		}
		if (parms.faction == Faction.OfMechanoids)
		{
			return ModsConfig.RoyaltyActive;
		}
		return false;
	}

	public override void TryGenerateThreats(IncidentParms parms)
	{
		parms.mechClusterSketch = MechClusterGenerator.GenerateClusterSketch(parms.points, parms.target as Map);
	}

	public override List<Pawn> SpawnThreats(IncidentParms parms)
	{
		return MechClusterUtility.SpawnCluster(parms.spawnCenter, (Map)parms.target, parms.mechClusterSketch, dropInPods: true, canAssaultColony: true, parms.questTag).OfType<Pawn>().ToList();
	}

	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		return null;
	}

	public override void MakeLords(IncidentParms parms, List<Pawn> pawns)
	{
	}
}
