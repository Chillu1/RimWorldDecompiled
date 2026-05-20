using System;
using System.Collections.Generic;

namespace RimWorld;

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
	public int Compare(TKey x, TKey y)
	{
		int num = x.CompareTo(y);
		if (num == 0)
		{
			return -1;
		}
		return num;
	}
}
