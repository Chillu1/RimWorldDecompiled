using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_ExtractTree : Designator
{
	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_ExtractTree()
	{
		defaultLabel = "DesignatorExtractTree".Translate();
		defaultDesc = "DesignatorExtractTreeDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/ExtractTree");
		useMouseIcon = true;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		soundSucceeded = SoundDefOf.Designate_ExtractTree;
		hotKey = KeyBindingDefOf.Misc12;
		tutorTag = "ExtractTree";
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		foreach (Thing thing in c.GetThingList(base.Map))
		{
			if (CanDesignateThing(thing).Accepted)
			{
				return true;
			}
		}
		return false;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t is Plant plant && plant.def.Minifiable && plant.def.plant.IsTree && base.Map.designationManager.DesignationOn(plant, DesignationDefOf.ExtractTree) == null)
		{
			return true;
		}
		return false;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		foreach (Thing thing in c.GetThingList(base.Map))
		{
			if (CanDesignateThing(thing).Accepted)
			{
				DesignateThing(thing);
			}
		}
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.AddDesignation(new Designation((Plant)t, DesignationDefOf.ExtractTree));
		Designation designation = base.Map.designationManager.DesignationOn(t, DesignationDefOf.CutPlant);
		if (designation != null)
		{
			base.Map.designationManager.RemoveDesignation(designation);
		}
		designation = base.Map.designationManager.DesignationOn(t, DesignationDefOf.HarvestPlant);
		if (designation != null)
		{
			base.Map.designationManager.RemoveDesignation(designation);
		}
	}
}
