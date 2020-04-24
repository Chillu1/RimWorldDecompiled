using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class Designator_Plan : Designator
	{
		private DesignateMode mode;

		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		protected override DesignationDef Designation => DesignationDefOf.Plan;

		public Designator_Plan(DesignateMode mode)
		{
			this.mode = mode;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			hotKey = KeyBindingDefOf.Misc9;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (c.InNoBuildEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
			if (mode == DesignateMode.Add)
			{
				if (base.Map.designationManager.DesignationAt(c, Designation) != null)
				{
					return false;
				}
			}
			else if (mode == DesignateMode.Remove && base.Map.designationManager.DesignationAt(c, Designation) == null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (mode == DesignateMode.Add)
			{
				base.Map.designationManager.AddDesignation(new Designation(c, Designation));
			}
			else if (mode == DesignateMode.Remove)
			{
				base.Map.designationManager.DesignationAt(c, Designation).Delete();
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			GenDraw.DrawNoBuildEdgeLines();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
