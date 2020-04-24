using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class Designator_Zone : Designator
	{
		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			GenUI.RenderMouseoverBracket();
			OverlayDrawHandler.DrawZonesThisFrame();
			if (Find.Selector.SelectedZone != null)
			{
				GenDraw.DrawFieldEdges(Find.Selector.SelectedZone.Cells);
			}
			GenDraw.DrawNoZoneEdgeLines();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
