using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_UndergroundCave : TileMutatorWorker_Caves
{
	private const int MinCaveSize = 2000;

	private const int MaxTries = 100;

	private const float BaseWidth = 4f;

	private const int AllowBranchingAfterThisManyCells = 10;

	protected override float BranchChance => 0.5f;

	protected override float WidthOffsetPerCell => 0.01f;

	protected override float MinTunnelWidth => 1.6f;

	public TileMutatorWorker_UndergroundCave(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		directionNoise = new Perlin(0.002050000010058284, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		List<IntVec3> list = map.AllCells.ToList();
		int num = 0;
		while (hashSet.Count < 2000)
		{
			num++;
			hashSet.Clear();
			MapGenerator.Caves.Clear();
			IntVec3 start = list.RandomElement();
			float width = Rand.Range(3.2f, 4f);
			MapGenCavesUtility.CaveGenParms parms = MapGenCavesUtility.CaveGenParms.Default;
			parms.widthOffsetPerCell = WidthOffsetPerCell;
			parms.minTunnelWidth = MinTunnelWidth;
			parms.branchChance = BranchChance;
			parms.allowBranchingAfterThisManyCells = 10;
			MapGenCavesUtility.Dig(start, Rand.Range(0f, 360f), width, list, map, closed: true, directionNoise, parms, Rock, hashSet);
			if (num > 100)
			{
				Log.Error($"Underground cave generation exceeded {100} tries");
				break;
			}
		}
		bool Rock(IntVec3 cell)
		{
			return TileMutatorWorker_Caves.IsRock(cell, MapGenerator.Elevation, map);
		}
	}
}
