using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_ExtractSkull : Designator
{
	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public override bool Visible
	{
		get
		{
			if (ModsConfig.IdeologyActive && WorkGiver_ExtractSkull.CanPlayerExtractSkull())
			{
				return true;
			}
			return false;
		}
	}

	public Designator_ExtractSkull()
	{
		defaultLabel = "DesignatorExtractSkull".Translate();
		defaultDesc = "DesignatorExtractSkullDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/ExtractSkull");
		useMouseIcon = true;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		soundSucceeded = SoundDefOf.Designate_ExtractSkull;
		hotKey = KeyBindingDefOf.Misc3;
		tutorTag = "ExtractSkull";
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
		if (!Visible)
		{
			return false;
		}
		if (!(t is Corpse corpse))
		{
			return false;
		}
		if (!corpse.InnerPawn.RaceProps.Humanlike)
		{
			return false;
		}
		if (!corpse.InnerPawn.health.hediffSet.GetNotMissingParts().Any((BodyPartRecord p) => p.def == BodyPartDefOf.Head))
		{
			return false;
		}
		if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.ExtractSkull) != null)
		{
			return false;
		}
		if (t.SpawnedParentOrMe is Building)
		{
			return false;
		}
		return true;
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
		base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.ExtractSkull));
	}
}
