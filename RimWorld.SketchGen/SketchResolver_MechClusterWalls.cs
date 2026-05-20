using System;
using UnityEngine;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_MechClusterWalls : SketchResolver
{
	private static readonly FloatRange WallCountRandomFactorRange = new FloatRange(0.5f, 1f);

	private static readonly SimpleCurve WidthToMaxWallsCountCurve = new SimpleCurve
	{
		new CurvePoint(3f, 1f),
		new CurvePoint(6f, 2f),
		new CurvePoint(9f, 3f),
		new CurvePoint(14f, 4f)
	};

	private const float Straight_LengthMinSizeFraction = 0.8f;

	private const float Corner_LengthMinSizeFraction = 0.4f;

	private const float EdgeWallChance = 0.8f;

	private const int MinWallLengthStraight = 3;

	private const int MinWallLengthCorner = 2;

	protected override void ResolveInt(SketchResolveParams parms)
	{
		IntVec2 value = parms.mechClusterSize.Value;
		int val = GenMath.RoundRandom((float)GenMath.RoundRandom(WidthToMaxWallsCountCurve.Evaluate(Mathf.Min(value.x, value.z))) * WallCountRandomFactorRange.RandomInRange);
		val = Math.Max(1, val);
		for (int i = 0; i < val; i++)
		{
			TryAddWall(parms.sketch, value);
		}
		if (Rand.Bool)
		{
			SketchResolveParams parms2 = parms;
			parms2.symmetryVertical = false;
			parms2.symmetryOrigin = value.x / 2;
			parms2.symmetryOriginIncluded = value.x % 2 == 1;
			SketchResolverDefOf.Symmetry.Resolve(parms2);
		}
		else if (Rand.Bool)
		{
			SketchResolveParams parms3 = parms;
			parms3.symmetryVertical = true;
			parms3.symmetryOrigin = value.z / 2;
			parms3.symmetryOriginIncluded = value.z % 2 == 1;
			SketchResolverDefOf.Symmetry.Resolve(parms3);
		}
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}

	private void TryAddWall(Sketch sketch, IntVec2 size)
	{
		for (int i = 0; i < 50; i++)
		{
			if (Rand.Chance(0.8f))
			{
				bool num = Rand.Bool;
				int num2 = (num ? size.x : size.z);
				CellRect rect;
				if (num)
				{
					IntVec2 intVec = new IntVec2(1, Rand.Bool ? (size.z - 1) : 0);
					rect = new CellRect(intVec.x, intVec.z, num2 - 1, 1);
				}
				else
				{
					IntVec2 intVec2 = new IntVec2(Rand.Bool ? (size.x - 1) : 0, 0);
					rect = new CellRect(intVec2.x, intVec2.z, 1, num2);
				}
				rect.ClipInsideRect(new CellRect(0, 0, size.x, size.z));
				if (rect.Area >= 3 && WallRectIsUsable(rect, checkAdjacentCells: false))
				{
					GenerateWallInRect(rect, Rand.Bool);
					break;
				}
				continue;
			}
			IntVec3 intVec3 = new IntVec3(Rand.RangeInclusive(0, size.x - 1), 0, Rand.RangeInclusive(0, size.z - 1));
			int num3 = GenMath.RoundRandom(Rand.Range((float)size.x * 0.4f, size.x));
			CellRect rect2 = ((!Rand.Bool) ? new CellRect(intVec3.x - num3 + 1, intVec3.z, num3, 1) : new CellRect(intVec3.x, intVec3.z, num3, 1));
			rect2.ClipInsideRect(new CellRect(0, 0, size.x, size.z));
			if (rect2.Area >= 2)
			{
				int num4 = GenMath.RoundRandom(Rand.Range((float)size.z * 0.4f, size.z));
				CellRect rect3 = ((!Rand.Bool) ? new CellRect(intVec3.x, intVec3.z - num4 + 1, 1, num4) : new CellRect(intVec3.x, intVec3.z, 1, num4));
				rect3.ClipInsideRect(new CellRect(0, 0, size.x, size.z));
				if (rect3.Area >= 2 && WallRectIsUsable(rect2, checkAdjacentCells: true) && WallRectIsUsable(rect3, checkAdjacentCells: true))
				{
					GenerateWallInRect(rect2, createRandomGap: false);
					GenerateWallInRect(rect3, createRandomGap: false);
					break;
				}
			}
		}
		void GenerateWallInRect(CellRect cellRect, bool createRandomGap)
		{
			IntVec3 randomCell = cellRect.RandomCell;
			foreach (IntVec3 item in cellRect)
			{
				if (!createRandomGap || !(item == randomCell))
				{
					sketch.AddThing(ThingDefOf.Wall, item, Rot4.North, ThingDefOf.Steel);
				}
			}
		}
		bool WallRectIsUsable(CellRect cellRect, bool checkAdjacentCells)
		{
			foreach (IntVec3 item2 in cellRect)
			{
				if (checkAdjacentCells)
				{
					for (int j = 0; j < 9; j++)
					{
						IntVec3 pos = item2 + GenAdj.AdjacentCellsAndInside[j];
						if (sketch.EdificeAt(pos) != null)
						{
							return false;
						}
					}
				}
				else if (sketch.EdificeAt(item2) != null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
