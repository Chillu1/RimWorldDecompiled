using System.Collections.Generic;

namespace Verse
{
	public abstract class Designator_Cells : Designator
	{
		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
