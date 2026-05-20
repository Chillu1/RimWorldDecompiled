using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace Verse.Glow;

public struct GlowCellComparer : IComparer<IntVec3>
{
	private UnsafeList<GlowCell> area;

	private GlowLight light;

	public GlowCellComparer(UnsafeList<GlowCell> area, in GlowLight light)
	{
		this.area = area;
		this.light = light;
	}

	public int Compare(IntVec3 a, IntVec3 b)
	{
		int index = light.DeltaToLocalIndex(in a);
		int index2 = light.DeltaToLocalIndex(in b);
		return area[index].intDist.CompareTo(area[index2].intDist);
	}
}
