using Verse;

namespace RimWorld;

public abstract class Designator_Zone : Designator_Cells
{
	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Zones;

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
}
