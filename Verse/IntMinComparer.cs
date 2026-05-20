using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Verse;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct IntMinComparer : IComparer<int>
{
	public int Compare(int x, int y)
	{
		if (x < y)
		{
			return -1;
		}
		if (x > y)
		{
			return 1;
		}
		return 0;
	}
}
