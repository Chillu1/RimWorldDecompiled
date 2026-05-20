using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GenStep_CaveHives : GenStep
{
	private List<IntVec3> rockCells = new List<IntVec3>();

	private List<IntVec3> possibleSpawnCells = new List<IntVec3>();

	private List<Hive> spawnedHives = new List<Hive>();

	private const int MinDistToOpenSpace = 10;

	private const int MinDistFromFactionBase = 50;

	private const float CaveCellsPerHive = 1000f;

	public override int SeedPart => 349641510;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!Find.Storyteller.difficulty.allowCaveHives || Faction.OfInsects == null)
		{
			return;
		}
		MapGenFloatGrid caves = MapGenerator.Caves;
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		float num = 0.7f;
		int num2 = 0;
		rockCells.Clear();
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (elevation[allCell] > num)
			{
				rockCells.Add(allCell);
			}
			if (caves[allCell] > 0f)
			{
				num2++;
			}
		}
		List<IntVec3> list = map.AllCells.Where((IntVec3 c) => map.thingGrid.ThingsAt(c).Any((Thing thing) => thing.Faction != null)).ToList();
		GenMorphology.Dilate(list, 50, map);
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>(list);
		int num3 = GenMath.RoundRandom((float)num2 / 1000f);
		GenMorphology.Erode(rockCells, 10, map);
		possibleSpawnCells.Clear();
		for (int num4 = 0; num4 < rockCells.Count; num4++)
		{
			if (caves[rockCells[num4]] > 0f && !hashSet.Contains(rockCells[num4]))
			{
				possibleSpawnCells.Add(rockCells[num4]);
			}
		}
		spawnedHives.Clear();
		for (int num5 = 0; num5 < num3; num5++)
		{
			TrySpawnHive(map);
		}
		spawnedHives.Clear();
	}

	private void TrySpawnHive(Map map)
	{
		if (TryFindHiveSpawnCell(map, out var spawnCell))
		{
			possibleSpawnCells.Remove(spawnCell);
			Hive item = HiveUtility.SpawnHive(spawnCell, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: true, canSpawnHives: false, canSpawnInsects: false, dormant: false, aggressive: false);
			spawnedHives.Add(item);
		}
	}

	private bool TryFindHiveSpawnCell(Map map, out IntVec3 spawnCell)
	{
		float num = -1f;
		IntVec3 intVec = IntVec3.Invalid;
		for (int i = 0; i < 3; i++)
		{
			if (!possibleSpawnCells.Where((IntVec3 x) => x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null).TryRandomElement(out var result))
			{
				break;
			}
			float num2 = -1f;
			for (int num3 = 0; num3 < spawnedHives.Count; num3++)
			{
				float num4 = result.DistanceToSquared(spawnedHives[num3].Position);
				if (num2 < 0f || num4 < num2)
				{
					num2 = num4;
				}
			}
			if (!intVec.IsValid || num2 > num)
			{
				intVec = result;
				num = num2;
			}
		}
		spawnCell = intVec;
		return spawnCell.IsValid;
	}
}
