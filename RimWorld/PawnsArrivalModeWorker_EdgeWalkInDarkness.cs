using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_EdgeWalkInDarkness : PawnsArrivalModeWorker
{
	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		Map map = (Map)parms.target;
		for (int i = 0; i < pawns.Count; i++)
		{
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8, (IntVec3 x) => map.glowGrid.PsychGlowAt(x) == PsychGlow.Dark);
			if (!loc.IsValid)
			{
				loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8);
			}
			GenSpawn.Spawn(pawns[i], loc, map, parms.spawnRotation);
		}
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Ignore, allowFogged: false, (IntVec3 x) => map.glowGrid.PsychGlowAt(x) == PsychGlow.Dark) && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Ignore))
		{
			return false;
		}
		parms.spawnRotation = Rot4.FromAngleFlat((map.Center - parms.spawnCenter).AngleFlat);
		return true;
	}
}
