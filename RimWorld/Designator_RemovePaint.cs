using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_RemovePaint : Designator_Cells
{
	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Paint;

	public Designator_RemovePaint()
	{
		defaultLabel = "DesignatorRemovePaint".Translate();
		defaultDesc = "DesignatorRemovePaintDesc".Translate();
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
		if ((from t in c.GetThingList(base.Map)
			where CanDesignateThing(t).Accepted
			select t).Any())
		{
			return AcceptanceReport.WasAccepted;
		}
		Building edifice = c.GetEdifice(base.Map);
		if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
		{
			return "MessageMustDesignatePainted".Translate();
		}
		if (base.Map.terrainGrid.ColorAt(c) == null)
		{
			return "MessageMustDesignatePainted".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (CanDesignateThing(thingList[i]).Accepted)
			{
				DesignateThing(thingList[i]);
				return;
			}
		}
		if (DebugSettings.godMode)
		{
			base.Map.terrainGrid.SetTerrainColor(c, null);
		}
		else
		{
			base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemovePaintFloor));
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t.Faction != Faction.OfPlayer)
		{
			return false;
		}
		if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Uninstall) != null)
		{
			return false;
		}
		return t is Building building && building.PaintColorDef != null;
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.TryRemoveDesignationOn(t, DesignationDefOf.PaintBuilding);
		base.Map.designationManager.TryRemoveDesignationOn(t, DesignationDefOf.RemovePaintBuilding);
		if (DebugSettings.godMode)
		{
			((Building)t).ChangePaint(null);
		}
		else
		{
			base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.RemovePaintBuilding));
		}
	}

	public override void RenderHighlight(List<IntVec3> dragCells)
	{
		DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
	}
}
