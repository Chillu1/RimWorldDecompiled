using Verse;

namespace RimWorld;

public class IncidentWorker_PoluxTreeSpawn : IncidentWorker_SpecialTreeSpawn
{
	protected override bool TryFindRootCell(Map map, out IntVec3 cell)
	{
		if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => x.IsPolluted(map) && base.GenStep.CanSpawnAt(x, map), map, out cell))
		{
			return true;
		}
		return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => base.GenStep.CanSpawnAt(x, map, 10, 0, 0f, 18, 20), map, out cell);
	}
}
