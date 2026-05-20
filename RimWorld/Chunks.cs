using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld;

public class Chunks
{
	private readonly int chunksX;

	public readonly int regionSize;

	public readonly CellRect rect;

	public readonly CellRect boundary;

	private readonly CellRect[] data;

	private readonly HashSet<IntVec3> used = new HashSet<IntVec3>();

	private readonly List<CellRect> enumerated = new List<CellRect>();

	private int usedCount;

	public CellRect this[IntVec3 cell] => CellToRect(cell);

	public CellRect this[int index] => data[index];

	public int UsedCount => usedCount;

	public int ApproximateArea => usedCount * regionSize * regionSize;

	public IReadOnlyList<CellRect> EnumeratedRects
	{
		get
		{
			if (enumerated.Empty())
			{
				RecacheEnumeratedChunks();
			}
			return enumerated;
		}
	}

	public IntVec3 RandomChunk => rect.RandomCell;

	public IntVec3 RandomUsedChunk => used.RandomElement();

	public Chunks(CellRect rect, int regionSize)
	{
		int subdivisionCount = MapGenUtility.GetSubdivisionCount(rect, ref regionSize);
		this.regionSize = regionSize;
		chunksX = 1;
		for (int i = 0; i < subdivisionCount; i++)
		{
			chunksX *= 2;
		}
		data = new CellRect[chunksX * chunksX];
		used.EnsureCapacity(data.Length);
		this.rect = new CellRect(0, 0, chunksX, chunksX);
		for (int j = 0; j < data.Length; j++)
		{
			IntVec3 intVec = IndexToCell(j);
			IntVec3 first = rect.Min + intVec * regionSize;
			IntVec3 second = rect.Min + intVec * regionSize + new IntVec3(regionSize, 0, regionSize);
			data[j] = CellRect.FromLimits(first, second);
			boundary = ((boundary == default(CellRect)) ? data[j] : boundary.Encapsulate(data[j]));
		}
	}

	public bool Contains(int ind)
	{
		if (ind >= 0)
		{
			return ind < chunksX * chunksX;
		}
		return false;
	}

	public bool Contains(IntVec3 cell)
	{
		return rect.Contains(cell);
	}

	public bool Used(int ind)
	{
		if (Contains(ind))
		{
			return Used(IndexToCell(ind));
		}
		return false;
	}

	public bool Used(IntVec3 cell)
	{
		if (Contains(cell))
		{
			return used.Contains(cell);
		}
		return false;
	}

	public void SetUsed(int index, bool set)
	{
		if (Contains(index))
		{
			SetUsed(IndexToCell(index), set);
		}
	}

	public void SetUsed(IntVec3 cell, bool set)
	{
		if (Contains(cell))
		{
			if (set && !used.Contains(cell))
			{
				usedCount++;
				used.Add(cell);
				enumerated.Clear();
			}
			else if (!set && used.Contains(cell))
			{
				usedCount--;
				used.Remove(cell);
				enumerated.Clear();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellRect CellToRect(IntVec3 c)
	{
		return data[CellToIndex(c)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntVec3 IndexToCell(int ind)
	{
		return CellIndicesUtility.IndexToCell(ind, chunksX);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CellToIndex(IntVec3 c)
	{
		return CellIndicesUtility.CellToIndex(c, chunksX);
	}

	public bool TryGetFreeEdge(out (IntVec3 chunk, Rot4 rot) edge, bool inBounds = false, int spaces = 1, Predicate<IntVec3> validator = null)
	{
		List<(IntVec3, Rot4)> list = new List<(IntVec3, Rot4)>();
		foreach (IntVec3 item in used.InRandomOrder())
		{
			if (TryGetFreeEdge(item, out var rot, inBounds, spaces, validator))
			{
				list.Add((item, rot));
			}
		}
		if (list.Empty())
		{
			edge = default((IntVec3, Rot4));
			return false;
		}
		edge = list.RandomElement();
		return true;
	}

	public bool TryGetFreeEdge(IntVec3 chunk, out Rot4 rot, bool inBounds = false, int spaces = 1, Predicate<IntVec3> validator = null)
	{
		List<Rot4> list = new List<Rot4>();
		for (int i = 0; i < 4; i++)
		{
			rot = new Rot4(i);
			bool flag = true;
			for (int j = 0; j < spaces; j++)
			{
				IntVec3 intVec = chunk + rot.FacingCell * (j + 1);
				if (Used(intVec) || (inBounds && !rect.Contains(intVec)) || (validator != null && !validator(intVec)))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(rot);
			}
		}
		if (list.Empty())
		{
			rot = default(Rot4);
			return false;
		}
		rot = list.RandomElement();
		return true;
	}

	private void RecacheEnumeratedChunks()
	{
		enumerated.Clear();
		foreach (CellRect item in rect.EnumerateRectanglesCovering(Validator))
		{
			CellRect cellRect = CellRect.Empty;
			foreach (IntVec3 cell in item.Cells)
			{
				CellRect cellRect2 = this[cell];
				cellRect = ((cellRect == CellRect.Empty) ? cellRect2 : cellRect.Encapsulate(cellRect2));
			}
			enumerated.Add(cellRect);
		}
		enumerated.SortByDescending((CellRect r) => r.Area);
		bool Validator(IntVec3 index)
		{
			return Used(index);
		}
	}
}
