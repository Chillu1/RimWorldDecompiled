using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_SmoothSurface : Designator
	{
		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_SmoothSurface()
		{
			defaultLabel = "DesignatorSmoothSurface".Translate();
			defaultDesc = "DesignatorSmoothSurfaceDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/SmoothSurface");
			useMouseIcon = true;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			soundSucceeded = SoundDefOf.Designate_SmoothSurface;
			hotKey = KeyBindingDefOf.Misc5;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (t != null && t.def.IsSmoothable && CanDesignateCell(t.Position).Accepted)
			{
				return AcceptanceReport.WasAccepted;
			}
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			DesignateSingleCell(t.Position);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (c.Fogged(base.Map))
			{
				return false;
			}
			if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor) != null || base.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothWall) != null)
			{
				return "SurfaceBeingSmoothed".Translate();
			}
			if (c.InNoBuildEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && edifice.def.IsSmoothable)
			{
				return AcceptanceReport.WasAccepted;
			}
			if (edifice != null && !SmoothSurfaceDesignatorUtility.CanSmoothFloorUnder(edifice))
			{
				return "MessageMustDesignateSmoothableSurface".Translate();
			}
			if (!c.GetTerrain(base.Map).affordances.Contains(TerrainAffordanceDefOf.SmoothableStone))
			{
				return "MessageMustDesignateSmoothableSurface".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && edifice.def.IsSmoothable)
			{
				base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.SmoothWall));
				base.Map.designationManager.TryRemoveDesignation(c, DesignationDefOf.Mine);
			}
			else
			{
				base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.SmoothFloor));
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
