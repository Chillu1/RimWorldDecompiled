using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Verse;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FloatMinComparer : IComparer<float>
{
	public int Compare(float x, float y)
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
