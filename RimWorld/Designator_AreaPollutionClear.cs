using Verse;

namespace RimWorld;

public abstract class Designator_AreaPollutionClear : Designator_Cells
{
	private DesignateMode mode;

	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;

	public Designator_AreaPollutionClear(DesignateMode mode)
	{
		this.mode = mode;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		bool flag = base.Map.areaManager.PollutionClear[c];
		if (mode == DesignateMode.Add)
		{
			return !flag;
		}
		return flag;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		if (mode == DesignateMode.Add)
		{
			base.Map.areaManager.PollutionClear[c] = true;
		}
		else
		{
			base.Map.areaManager.PollutionClear[c] = false;
		}
	}

	public override void SelectedUpdate()
	{
		GenUI.RenderMouseoverBracket();
		base.Map.areaManager.PollutionClear.MarkForDraw();
	}
}
