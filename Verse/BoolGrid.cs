using System;
using System.Collections.Generic;

namespace Verse;

public class BoolGrid : IExposable
{
	private bool[] arr;

	private int trueCountInt;

	private int width;

	private int height;

	private int minPossibleTrueIndexCached = -1;

	private bool minPossibleTrueIndexDirty;

	public int TrueCount => trueCountInt;

	public int Width => width;

	public int Height => height;

	public IEnumerable<IntVec3> ActiveCells
	{
		get
		{
			if (trueCountInt == 0)
			{
				yield break;
			}
			int yieldedCount = 0;
			bool canSetMinPossibleTrueIndex = minPossibleTrueIndexDirty;
			int num = ((!minPossibleTrueIndexDirty) ? minPossibleTrueIndexCached : 0);
			for (int i = num; i < arr.Length; i++)
			{
				if (arr[i])
				{
					if (canSetMinPossibleTrueIndex && minPossibleTrueIndexDirty)
					{
						canSetMinPossibleTrueIndex = false;
						minPossibleTrueIndexDirty = false;
						minPossibleTrueIndexCached = i;
					}
					yield return CellIndicesUtility.IndexToCell(i, width);
					yieldedCount++;
					if (yieldedCount >= trueCountInt)
					{
						break;
					}
				}
			}
		}
	}

	public bool this[int index]
	{
		get
		{
			return arr[index];
		}
		set
		{
			Set(index, value);
		}
	}

	public bool this[IntVec3 c]
	{
		get
		{
			return arr[CellIndicesUtility.CellToIndex(c, width)];
		}
		set
		{
			Set(c, value);
		}
	}

	public bool this[int x, int z]
	{
		get
		{
			return arr[CellIndicesUtility.CellToIndex(x, z, width)];
		}
		set
		{
			Set(CellIndicesUtility.CellToIndex(x, z, width), value);
		}
	}

	public BoolGrid()
	{
	}

	public BoolGrid(Map map)
	{
		ClearAndResizeTo(map);
	}

	public BoolGrid(int x, int z)
	{
		ClearAndResizeTo(x, z);
	}

	public bool MapSizeMatches(Map map)
	{
		if (width == map.Size.x)
		{
			return height == map.Size.z;
		}
		return false;
	}

	public void ClearAndResizeTo(Map map)
	{
		if (MapSizeMatches(map))
		{
			if (arr != null)
			{
				Clear();
			}
		}
		else
		{
			ClearAndResizeTo(map.Size.x, map.Size.z);
		}
	}

	public void ClearAndResizeTo(int x, int z)
	{
		if (width == x && height == z)
		{
			if (arr != null)
			{
				Clear();
			}
			return;
		}
		width = x;
		height = z;
		arr = new bool[width * height];
		trueCountInt = 0;
		minPossibleTrueIndexCached = -1;
		minPossibleTrueIndexDirty = false;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref trueCountInt, "trueCount", 0);
		Scribe_Values.Look(ref width, "mapSizeX", 0);
		Scribe_Values.Look(ref height, "mapSizeZ", 0);
		DataExposeUtility.LookBoolArray(ref arr, width * height, "arr");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			minPossibleTrueIndexDirty = true;
		}
	}

	public void Clear()
	{
		Array.Clear(arr, 0, arr.Length);
		trueCountInt = 0;
		minPossibleTrueIndexCached = -1;
		minPossibleTrueIndexDirty = false;
	}

	public virtual void Set(IntVec3 c, bool value)
	{
		Set(CellIndicesUtility.CellToIndex(c, width), value);
	}

	public virtual void Set(int index, bool value)
	{
		if (arr[index] == value)
		{
			return;
		}
		arr[index] = value;
		if (value)
		{
			trueCountInt++;
			if (trueCountInt == 1 || index < minPossibleTrueIndexCached)
			{
				minPossibleTrueIndexCached = index;
			}
		}
		else
		{
			trueCountInt--;
			if (index == minPossibleTrueIndexCached)
			{
				minPossibleTrueIndexDirty = true;
			}
		}
	}

	public void Invert()
	{
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = !arr[i];
		}
		trueCountInt = arr.Length - trueCountInt;
		minPossibleTrueIndexDirty = true;
	}
}
