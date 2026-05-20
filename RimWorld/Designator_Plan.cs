using Verse;

namespace RimWorld;

public abstract class Designator_Plan : Designator_Cells
{
	public override bool DragDrawMeasurements => true;

	protected override bool DoTooltip => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Plans;

	public override void SelectedUpdate()
	{
		base.SelectedUpdate();
		GenUI.RenderMouseoverBracket();
		GenDraw.DrawNoZoneEdgeLines();
	}
}
