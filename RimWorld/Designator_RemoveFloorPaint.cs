using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_RemoveFloorPaint : Designator_Cells
{
	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Paint;

	public Designator_RemoveFloorPaint()
	{
		defaultLabel = "DesignatorRemoveFloorPaint".Translate();
		defaultDesc = "DesignatorRemoveFloorPaintDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/RemovePaint");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_RemovePaint;
		hotKey = KeyBindingDefOf.Misc7;
		tutorTag = "RemovePaint";
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map) || c.Fogged(base.Map))
		{
			return false;
		}
		if (base.Map.terrainGrid.ColorAt(c) == null)
		{
			return "MessageMustDesignatePainted".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		if (DebugSettings.godMode)
		{
			base.Map.terrainGrid.SetTerrainColor(c, null);
		}
		else
		{
			base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemovePaintFloor));
		}
	}

	public override void RenderHighlight(List<IntVec3> dragCells)
	{
		DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
	}
}
