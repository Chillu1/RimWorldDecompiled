using Verse;

namespace RimWorld;

public class IncidentWorker_HarbingerTreeSpawn : IncidentWorker_SpecialTreeSpawn
{
	protected override int MaxTreesPerIncident => 2;

	protected override bool TryFindRootCell(Map map, out IntVec3 cell)
	{
		return TryGetHarbingerTreeSpawnCell(map, out cell);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		bool num = base.TryExecuteWorker(parms);
		if (num)
		{
			Find.Anomaly.Notify_HarbingerTreeSpawned();
		}
		return num;
	}

	public static bool TryGetHarbingerTreeSpawnCell(Map map, out IntVec3 cell, IntVec3? nearLoc = null)
	{
		GenStep_SpecialTrees genStep = (GenStep_SpecialTrees)GenStepDefOf.HarbingerTrees.genStep;
		int num = Current.Game.AnyPlayerHomeMap?.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeHarbinger).Count ?? 0;
		int maxProximityToSameTree = ((num > 0) ? 7 : (-1));
		if (nearLoc.HasValue)
		{
			if (RCellFinder.TryFindRandomCellNearWith(nearLoc.Value, (IntVec3 x) => genStep.CanSpawnAt(x, map, 20, 0, 0.5f, 18, 20, 3, maxProximityToSameTree), map, out cell, 5, maxProximityToSameTree))
			{
				return true;
			}
			if (RCellFinder.TryFindRandomCellNearWith(nearLoc.Value, (IntVec3 x) => genStep.CanSpawnAt(x, map, 2, 0, 0.5f, 22, 10, 3, maxProximityToSameTree), map, out cell, 5, maxProximityToSameTree))
			{
				return true;
			}
		}
		if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => genStep.CanSpawnAt(x, map, 20, 0, 0.5f, 18, 20, 3, maxProximityToSameTree), map, 1000, out cell))
		{
			return true;
		}
		if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => genStep.CanSpawnAt(x, map, 2, 0, 0.5f, 22, 10, 3, maxProximityToSameTree), map, 10000, out cell))
		{
			return true;
		}
		if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => genStep.CanSpawnAt(x, map, 20, 0, 0.5f, 18, 20, 3), map, 1000, out cell))
		{
			return true;
		}
		return CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => genStep.CanSpawnAt(x, map, 2, 0, 0.5f, 22, 10, 3), map, 1000, out cell);
	}
}
