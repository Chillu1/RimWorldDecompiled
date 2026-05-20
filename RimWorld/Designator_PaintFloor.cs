using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Designator_PaintFloor : Designator_Paint
	{
		protected override Texture2D IconTopTex => ContentFinder<Texture2D>.Get("UI/Designators/PaintFloor_Top");

		protected override DesignationDef Designation => DesignationDefOf.PaintFloor;

		public Designator_PaintFloor()
		{
			defaultLabel = "DesignatorPaintFloor".Translate();
			defaultDesc = "DesignatorPaintFloorDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/PaintFloor_Bottom");
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (eyedropMode)
			{
				return eyedropper.CanDesignateCell(c);
			}
			if (!c.InBounds(base.Map) || c.Fogged(base.Map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
			{
				return false;
			}
			if (!base.Map.terrainGrid.TerrainAt(c).isPaintable)
			{
				return "MessageMustDesignatePaintableFloors".Translate(colorDef);
			}
			if (base.Map.terrainGrid.ColorAt(c) == colorDef)
			{
				return false;
			}
			if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor) != null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (eyedropMode)
			{
				eyedropper.DesignateSingleCell(c);
				return;
			}
			if (DebugSettings.godMode)
			{
				base.Map.terrainGrid.SetTerrainColor(c, colorDef);
				return;
			}
			base.Map.designationManager.TryRemoveDesignation(c, Designation);
			base.Map.designationManager.TryRemoveDesignation(c, DesignationDefOf.RemovePaintFloor);
			base.Map.designationManager.AddDesignation(new Designation(c, Designation, colorDef));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}

		protected override int NumHighlightedCells()
		{
			return Find.DesignatorManager.Dragger.DragCells.Count;
		}
	}
}
