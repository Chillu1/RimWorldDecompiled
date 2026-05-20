using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_ScatterAncientFences : GenStep_Scatterer
{
	private static readonly FloatRange PerimeterCellsRange = new FloatRange(0.2f, 0.4f);

	private static readonly IntRange RectSizeRange = new IntRange(6, 12);

	private const float SkipChance = 0.25f;

	private CellRect rect;

	private ThingDef thingToScatter;

	private static readonly List<IntVec3> tmpFenceCells = new List<IntVec3>();

	private static readonly List<CellRect> tmpUsedRects = new List<CellRect>();

	public override int SeedPart => 344678634;

	protected override float GetPlacementFactor(Map map)
	{
		float num = 1f;
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			num *= mutator.junkDensityFactor;
		}
		return num;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter ancient fences"))
		{
			count = Rand.RangeInclusive(1, 3);
			tmpUsedRects.Clear();
			base.Generate(map, parms);
			tmpUsedRects.Clear();
			tmpFenceCells.Clear();
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		thingToScatter = (Rand.Bool ? ThingDefOf.AncientFence : ThingDefOf.AncientRazorWire);
		int randomInRange = RectSizeRange.RandomInRange;
		CellRect item = CellRect.CenteredOn(loc, randomInRange, randomInRange);
		for (int i = 0; i < tmpUsedRects.Count; i++)
		{
			if (item.Overlaps(tmpUsedRects[i]))
			{
				return false;
			}
		}
		tmpFenceCells.Clear();
		tmpFenceCells.AddRange(item.EdgeCells);
		for (int j = 0; j < tmpFenceCells.Count; j++)
		{
			if (!tmpFenceCells[j].InBounds(map) || tmpFenceCells[j].GetEdifice(map) != null || tmpFenceCells[j].GetRoof(map) != null || tmpFenceCells[j].Impassable(map))
			{
				return false;
			}
			TerrainDef terrain = tmpFenceCells[j].GetTerrain(map);
			if (terrain.IsWater || terrain.IsRoad || !GenConstruct.CanBuildOnTerrain(thingToScatter, tmpFenceCells[j], map, Rot4.North))
			{
				return false;
			}
		}
		tmpUsedRects.Add(item);
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		int num = Rand.Range(0, tmpFenceCells.Count);
		int num2 = Mathf.RoundToInt((float)tmpFenceCells.Count * PerimeterCellsRange.RandomInRange);
		for (int i = 0; i < num2; i++)
		{
			if (!Rand.Chance(0.25f))
			{
				IntVec3 loc2 = tmpFenceCells[(i + num) % tmpFenceCells.Count];
				GenSpawn.Spawn(ThingMaker.MakeThing(thingToScatter), loc2, map);
			}
		}
	}
}
