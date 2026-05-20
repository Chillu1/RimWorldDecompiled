using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class GridShapeMaker
{
	private static HashSet<IntVec3> edgeCells = new HashSet<IntVec3>();

	private static HashSet<IntVec3> shapeCells = new HashSet<IntVec3>();

	public static List<IntVec3> IrregularLump(IntVec3 center, Map map, int numCells, Predicate<IntVec3> validator = null)
	{
		HashSet<IntVec3> lumpCells = new HashSet<IntVec3>();
		for (int i = 0; i < numCells * 2; i++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[i];
			if (intVec.InBounds(map) && (validator == null || validator(intVec)))
			{
				lumpCells.Add(intVec);
			}
		}
		List<IntVec3> list = new List<IntVec3>();
		while (lumpCells.Count > numCells)
		{
			int num = 99;
			foreach (IntVec3 item in lumpCells)
			{
				int num2 = CountNeighbours(item);
				if (num2 < num)
				{
					num = num2;
				}
			}
			list.Clear();
			foreach (IntVec3 item2 in lumpCells)
			{
				if (CountNeighbours(item2) == num)
				{
					list.Add(item2);
				}
			}
			lumpCells.Remove(list.RandomElement());
		}
		return lumpCells.ToList();
		int CountNeighbours(IntVec3 sq)
		{
			int num3 = 0;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec2 in cardinalDirections)
			{
				if (lumpCells.Contains(sq + intVec2))
				{
					num3++;
				}
			}
			return num3;
		}
	}

	public static List<IntVec3> IrregularLumpRelative(int numCells)
	{
		HashSet<IntVec3> lumpCells = new HashSet<IntVec3>();
		for (int i = 0; i < numCells * 2; i++)
		{
			lumpCells.Add(GenRadial.RadialPattern[i]);
		}
		List<IntVec3> list = new List<IntVec3>();
		while (lumpCells.Count > numCells)
		{
			int num = 99;
			foreach (IntVec3 item in lumpCells)
			{
				int num2 = CountNeighbours(item);
				if (num2 < num)
				{
					num = num2;
				}
			}
			list.Clear();
			foreach (IntVec3 item2 in lumpCells)
			{
				if (CountNeighbours(item2) == num)
				{
					list.Add(item2);
				}
			}
			lumpCells.Remove(list.RandomElement());
		}
		return lumpCells.ToList();
		int CountNeighbours(IntVec3 sq)
		{
			int num3 = 0;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec in cardinalDirections)
			{
				if (lumpCells.Contains(sq + intVec))
				{
					num3++;
				}
			}
			return num3;
		}
	}

	public static List<IntVec3> UnnaturalShape(IntVec3 center, Map map, int approximateNumCells)
	{
		return (from c in UnnaturalShapeRelative(approximateNumCells)
			select center + c).ToList();
	}

	public static List<IntVec3> UnnaturalShapeRelative(int approximateNumCells)
	{
		edgeCells.Clear();
		shapeCells.Clear();
		int num = Mathf.Max(Mathf.FloorToInt(Mathf.Sqrt(approximateNumCells)), 1);
		int minInclusive = Mathf.Min(num, 4);
		while (shapeCells.Count < approximateNumCells)
		{
			IntVec3 center = new IntVec3(0, 0, 0);
			if (edgeCells.Count > 0)
			{
				center = edgeCells.RandomElement();
			}
			CellRect cellRect = CellRect.CenteredOn(center, Rand.Range(minInclusive, num), Rand.Range(minInclusive, num));
			edgeCells.AddRange(cellRect.ExpandedBy(1).EdgeCells);
			shapeCells.AddRange(cellRect.Cells);
			edgeCells.RemoveWhere((IntVec3 c) => shapeCells.Contains(c));
		}
		return shapeCells.ToList();
	}
}
