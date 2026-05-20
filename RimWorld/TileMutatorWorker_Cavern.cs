using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Cavern : TileMutatorWorker_Caves
{
	private const float OpeningNoiseFreq = 0.015f;

	private const float OpeningNoiseStrength = 40f;

	private const float OpeningRadius = 0.4f;

	private const float OpeningThreshold = 0.5f;

	private static readonly FloatRange OpeningSquashRange = new FloatRange(1f, 1.3f);

	private const float CoastFalloff = 0.1f;

	private const float HolesFrequency = 0.02f;

	private const float HolesThreshold = 0.7f;

	private const float BaseWidth = 7f;

	private const int AllowBranchingAfterThisManyCells = 10;

	private static readonly IntRange NumTunnelsRange = new IntRange(4, 6);

	private ModuleBase openingNoise;

	private ModuleBase holesNoise;

	protected override float BranchChance => 0.02f;

	protected override float WidthOffsetPerCell => 0.02f;

	protected override float MinTunnelWidth => 1.6f;

	public TileMutatorWorker_Cavern(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			openingNoise = new DistFromPoint((float)map.Size.x * 0.4f);
			openingNoise = new Scale(OpeningSquashRange.RandomInRange, 1.0, 1.0, openingNoise);
			openingNoise = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, openingNoise);
			openingNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, openingNoise);
			float? num = Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean) ?? Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Lake);
			if (num.HasValue)
			{
				ModuleBase rhs = MapNoiseUtility.FalloffAtAngle(num.Value, 0.1f, map);
				openingNoise = new SmoothMin(openingNoise, rhs, 0.2);
			}
			openingNoise = MapNoiseUtility.AddDisplacementNoise(openingNoise, 0.015f, 40f);
			holesNoise = new Perlin(0.019999999552965164, 2.0, 0.5, 2, Rand.Int, QualityMode.Medium);
			holesNoise = MapNoiseUtility.AddDisplacementNoise(holesNoise, 0.015f, 40f);
			base.Init(map);
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		MapGenFloatGrid caves = MapGenerator.Caves;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = openingNoise.GetValue(allCell);
			if (value > 0.5f)
			{
				elevation[allCell] = value;
			}
		}
		CreateTunnels(map);
		foreach (IntVec3 allCell2 in map.AllCells)
		{
			if (holesNoise.GetValue(allCell2) > 0.7f)
			{
				caves[allCell2] = 1f;
			}
		}
		RoofCollapseCellsFinder.CheckAndRemoveCollpsingRoofs(map);
	}

	private void CreateTunnels(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		List<IntVec3> list = FindLargestContiguousRockFormation(map);
		int randomInRange = NumTunnelsRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			IntVec3 start = IntVec3.Invalid;
			float num = -1f;
			float dir = -1f;
			float num2 = -1f;
			for (int j = 0; j < 10; j++)
			{
				IntVec3 intVec = MapGenCavesUtility.FindRandomEdgeCellForTunnel(list, map);
				float distToCave = MapGenCavesUtility.GetDistToCave(intVec, list, map, 40f, treatOpenSpaceAsCave: false);
				float dist;
				float num3 = MapGenCavesUtility.FindBestInitialDir(intVec, list, out dist);
				if (!start.IsValid || distToCave > num || (Mathf.Approximately(distToCave, num) && dist > num2))
				{
					start = intVec;
					num = distToCave;
					dir = num3;
					num2 = dist;
				}
			}
			float width = Rand.Range(5.6f, 7f);
			MapGenCavesUtility.CaveGenParms parms = MapGenCavesUtility.CaveGenParms.Default;
			parms.widthOffsetPerCell = WidthOffsetPerCell;
			parms.minTunnelWidth = MinTunnelWidth;
			parms.branchChance = BranchChance;
			parms.allowBranchingAfterThisManyCells = 10;
			parms.widthNoiseAmplitude = 2f;
			MapGenCavesUtility.Dig(start, dir, width, list, map, closed: false, directionNoise, parms, Rock);
		}
		bool Rock(IntVec3 cell)
		{
			return TileMutatorWorker_Caves.IsRock(cell, elevation, map);
		}
	}

	private List<IntVec3> FindLargestContiguousRockFormation(Map map)
	{
		HashSet<IntVec3> checkedCells = new HashSet<IntVec3>();
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		HashSet<IntVec3> current = new HashSet<IntVec3>();
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (TileMutatorWorker_Caves.IsRock(allCell, elevation, map) && !checkedCells.Contains(allCell))
			{
				current.Clear();
				map.floodFiller.FloodFill(allCell, (IntVec3 x) => TileMutatorWorker_Caves.IsRock(x, elevation, map), delegate(IntVec3 x)
				{
					current.Add(x);
					checkedCells.Add(x);
				});
				if (current.Count > hashSet.Count)
				{
					hashSet.Clear();
					hashSet.AddRange(current);
				}
			}
		}
		return hashSet.ToList();
	}
}
