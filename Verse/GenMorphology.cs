using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Noise;

namespace Verse;

public static class GenMorphology
{
	public struct GenPatchParms
	{
		public ThingDef edificeDef;

		public TerrainDef terrainDef;

		public RoofDef roofDef;

		public float macroFrequency;

		public float microFrequency;

		public float blendMacroMicro;

		public float blendFalloff;

		public float terrainThreshold;

		public float edificeThreshold;

		public float roofThreshold;

		public static GenPatchParms Default => For(ThingDefOf.Slate, ThingDefOf.Slate.building.naturalTerrain);

		public static GenPatchParms For(ThingDef edifice)
		{
			return For(edifice, null);
		}

		public static GenPatchParms For(TerrainDef terrain)
		{
			return For(null, terrain);
		}

		public static GenPatchParms For(ThingDef edifice, TerrainDef terrain)
		{
			return new GenPatchParms
			{
				edificeDef = edifice,
				terrainDef = terrain,
				roofDef = RoofDefOf.RoofRockThin,
				macroFrequency = 0.05f,
				microFrequency = 0.08f,
				blendMacroMicro = 0.01f,
				blendFalloff = 0.4f,
				terrainThreshold = 0.45f,
				edificeThreshold = 0.5f,
				roofThreshold = 0.55f
			};
		}
	}

	private static HashSet<IntVec3> tmpOutput = new HashSet<IntVec3>();

	private static HashSet<IntVec3> cellsSet = new HashSet<IntVec3>();

	private static List<IntVec3> tmpEdgeCells = new List<IntVec3>();

	private static readonly HashSet<IntVec3> visited = new HashSet<IntVec3>();

	public static void Erode(List<IntVec3> cells, int count, Map map, Predicate<IntVec3> extraPredicate = null)
	{
		if (count <= 0)
		{
			return;
		}
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		cellsSet.Clear();
		cellsSet.AddRange(cells);
		tmpEdgeCells.Clear();
		for (int i = 0; i < cells.Count; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				IntVec3 item = cells[i] + cardinalDirections[j];
				if (!cellsSet.Contains(item))
				{
					tmpEdgeCells.Add(cells[i]);
					break;
				}
			}
		}
		if (!tmpEdgeCells.Any())
		{
			return;
		}
		tmpOutput.Clear();
		Predicate<IntVec3> passCheck = ((extraPredicate == null) ? ((Predicate<IntVec3>)((IntVec3 x) => cellsSet.Contains(x))) : ((Predicate<IntVec3>)((IntVec3 x) => cellsSet.Contains(x) && extraPredicate(x))));
		map.floodFiller.FloodFill(IntVec3.Invalid, passCheck, delegate(IntVec3 cell, int traversalDist)
		{
			if (traversalDist >= count)
			{
				tmpOutput.Add(cell);
			}
			return false;
		}, int.MaxValue, rememberParents: false, tmpEdgeCells);
		cells.Clear();
		cells.AddRange(tmpOutput);
	}

	public static void Dilate(List<IntVec3> cells, int count, Map map, Predicate<IntVec3> extraPredicate = null)
	{
		if (count <= 0)
		{
			return;
		}
		map.floodFiller.FloodFill(IntVec3.Invalid, extraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => true)), delegate(IntVec3 cell, int traversalDist)
		{
			if (traversalDist > count)
			{
				return true;
			}
			if (traversalDist != 0)
			{
				cells.Add(cell);
			}
			return false;
		}, int.MaxValue, rememberParents: false, cells);
	}

	public static void Open(List<IntVec3> cells, int count, Map map)
	{
		Erode(cells, count, map);
		Dilate(cells, count, map);
	}

	public static void Close(List<IntVec3> cells, int count, Map map)
	{
		Dilate(cells, count, map);
		Erode(cells, count, map);
	}

	public static void GenerateNaturalPatch(Map map, CellRect rect, GenPatchParms? parms = null, Predicate<IntVec3> validator = null)
	{
		GenerateNaturalPatch(map, new List<CellRect> { rect }, parms, validator);
	}

	public static void GenerateNaturalPatch(Map map, List<CellRect> rects, GenPatchParms? parms = null, Predicate<IntVec3> validator = null)
	{
		GenPatchParms genPatchParms = parms ?? GenPatchParms.Default;
		Perlin lhs = new Perlin(genPatchParms.macroFrequency, 1.25, 0.5, 6, map.ConstantRandSeed, QualityMode.Medium);
		ModuleBase rhs = new Perlin(genPatchParms.microFrequency, 2.0, 0.5, 6, map.ConstantRandSeed + 1, QualityMode.Medium);
		ModuleBase input = new Blend(lhs, rhs, new Const(genPatchParms.blendMacroMicro));
		input = new ScaleBias(0.5, 0.6000000238418579, input);
		ModuleBase input2 = new DistFromPointRects(rects);
		input2 = new Clamp(0.0, 1.0, input2);
		input2 = new ScaleBias(2.200000047683716, -0.5, input2);
		input = new Blend(input2, input, new Const(genPatchParms.blendFalloff));
		foreach (CellRect rect in rects)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				if (visited.Add(cell) && cell.InBounds(map) && (validator == null || validator(cell)))
				{
					float value = input.GetValue(cell);
					if (genPatchParms.terrainDef != null && value >= genPatchParms.terrainThreshold)
					{
						map.terrainGrid.SetTerrain(cell, genPatchParms.terrainDef);
					}
					if (genPatchParms.edificeDef != null && value >= genPatchParms.edificeThreshold)
					{
						GenSpawn.Spawn(genPatchParms.edificeDef, cell, map);
					}
					if (genPatchParms.roofDef != null && value >= genPatchParms.roofThreshold)
					{
						map.roofGrid.SetRoof(cell, genPatchParms.roofDef);
					}
				}
			}
		}
		visited.Clear();
	}

	private static float ValueAt(ModuleBase noise, IntVec3 cell, List<CellRect> rects)
	{
		float value = rects.Min(delegate(CellRect rect)
		{
			IntVec3 intVec = rect.ClosestCellTo(cell);
			return (cell - intVec).Magnitude;
		});
		float num = Mathf.Clamp01(Mathf.InverseLerp(0f, 12f, value));
		return noise.GetValue(cell) + 1f - 2f * num;
	}
}
