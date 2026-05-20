using Verse;

namespace RimWorld;

public class IncidentWorker_Revenant : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		IntVec3 result = parms.spawnCenter;
		if (!result.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Hostile))
		{
			return false;
		}
		Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
		GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Revenant, Faction.OfEntities, PawnGenerationContext.NonPlayer, map.Tile)), result, map, rot);
		return true;
	}
}
