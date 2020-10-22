using Verse;

namespace RimWorld
{
	public class IncidentWorker_AnimaTreeSpawn : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			if (map.Biome.isExtremeBiome)
			{
				return false;
			}
			int num = GenStep_AnimaTrees.DesiredTreeCountForMap(map);
			if (map.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeAnima).Count >= num)
			{
				return false;
			}
			IntVec3 cell;
			return TryFindRootCell(map, out cell);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindRootCell(map, out var cell))
			{
				return false;
			}
			if (!GenStep_AnimaTrees.TrySpawnAt(cell, map, 0.05f, out var plant))
			{
				return false;
			}
			if (PawnsFinder.HomeMaps_FreeColonistsSpawned.Any((Pawn c) => c.HasPsylink && MeditationFocusDefOf.Natural.CanPawnUse(c)))
			{
				SendStandardLetter(parms, plant);
			}
			return true;
		}

		private bool TryFindRootCell(Map map, out IntVec3 cell)
		{
			if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => GenStep_AnimaTrees.CanSpawnAt(x, map), map, out cell))
			{
				return true;
			}
			return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => GenStep_AnimaTrees.CanSpawnAt(x, map, 10, 0, 18, 20), map, out cell);
		}
	}
}
