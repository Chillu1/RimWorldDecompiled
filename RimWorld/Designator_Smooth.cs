using Verse;

namespace RimWorld;

public abstract class Designator_Smooth : Designator_Cells
{
	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Orders;

	protected Designator_Smooth()
	{
		useMouseIcon = true;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		soundSucceeded = SoundDefOf.Designate_SmoothSurface;
		hotKey = KeyBindingDefOf.Misc5;
	}

	public override void SelectedUpdate()
	{
		GenUI.RenderMouseoverBracket();
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map) || c.Fogged(base.Map))
		{
			return false;
		}
		if (c.InNoBuildEdgeArea(base.Map))
		{
			return "TooCloseToMapEdge".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}
}
