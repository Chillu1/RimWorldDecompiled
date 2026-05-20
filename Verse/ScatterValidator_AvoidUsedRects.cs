using System.Collections.Generic;

namespace Verse;

public class ScatterValidator_AvoidUsedRects : ScattererValidator
{
	public IntVec2 size = new IntVec2(1, 1);

	public override bool Allows(IntVec3 c, Map map)
	{
		if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var))
		{
			return true;
		}
		CellRect cellRect = new CellRect(c.x, c.z, size.x, size.z);
		foreach (CellRect item in var)
		{
			if (cellRect.Overlaps(item))
			{
				return false;
			}
		}
		return true;
	}
}
