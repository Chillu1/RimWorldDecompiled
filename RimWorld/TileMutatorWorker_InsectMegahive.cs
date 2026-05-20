using Verse;

namespace RimWorld;

public class TileMutatorWorker_InsectMegahive : TileMutatorWorker
{
	private static readonly IntRange NumHivesRange = new IntRange(1, 2);

	public TileMutatorWorker_InsectMegahive(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostFog(Map map)
	{
		if (ModsConfig.OdysseyActive && TryGetEntranceCell(map, out var entranceCell))
		{
			GenerateMegahiveEntrance(map, entranceCell);
		}
	}

	public static void GenerateMegahiveEntrance(Map map, IntVec3 entranceCell, bool spawnGravcore = false)
	{
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(entranceCell, map, 150, (IntVec3 _) => true))
		{
			Building edifice = item.GetEdifice(map);
			if (edifice != null)
			{
				edifice.Destroy();
				map.fogGrid.FloodUnfogAdjacent(edifice, sendLetters: false);
			}
			map.terrainGrid.SetTerrain(item, GenStep_RocksFromGrid.RockDefAt(item).building.naturalTerrain);
			map.terrainGrid.SetTerrain(item, TerrainDefOf.InsectSludge);
			if (Rand.Chance(0.2f))
			{
				GenSpawn.Spawn(ThingDefOf.Filth_Slime, item, map);
			}
		}
		(GenSpawn.Spawn(ThingDefOf.InsectLairEntrance, entranceCell, map) as InsectLairEntrance).spawnGravcore = spawnGravcore;
		MapGenerator.UsedRects.Add(GenAdj.OccupiedRect(entranceCell, Rot4.North, 8, 8));
		int randomInRange = NumHivesRange.RandomInRange;
		for (int num = 0; num < randomInRange; num++)
		{
			if (CellFinder.TryFindRandomCellNear(entranceCell, map, 6, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.Hive, c, map), out var result))
			{
				HiveUtility.SpawnHive(result, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: true, canSpawnHives: false);
			}
		}
		FloodFillerFog.FloodUnfog(entranceCell, map);
	}

	public static bool TryGetEntranceCell(Map map, out IntVec3 entranceCell)
	{
		if (CellFinder.TryFindRandomCell(map, delegate(IntVec3 cell)
		{
			if (!GenSpawn.CanSpawnAt(ThingDefOf.InsectLairEntrance, cell, map))
			{
				return false;
			}
			if (!cell.Roofed(map))
			{
				return false;
			}
			return !cell.Fogged(map);
		}, out entranceCell))
		{
			return true;
		}
		if (CellFinder.TryFindRandomCell(map, delegate(IntVec3 cell)
		{
			if (!GenSpawn.CanSpawnAt(ThingDefOf.InsectLairEntrance, cell, map))
			{
				return false;
			}
			return !cell.Fogged(map);
		}, out entranceCell))
		{
			return true;
		}
		return false;
	}
}
