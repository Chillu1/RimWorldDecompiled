using System.Collections.Generic;

namespace Verse;

public class DrawStyle_FilledRectangle : DrawStyle
{
	public override bool CanHaveDuplicates => false;

	public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
	{
		buffer.AddRange(CellRect.FromLimits(origin, target).Cells);
	}
}
