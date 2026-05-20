using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Unforbid : Designator
{
	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Unforbid()
	{
		defaultLabel = "DesignatorUnforbid".Translate();
		defaultDesc = "DesignatorUnforbidDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOff");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Checkbox_TurnedOn;
		hotKey = KeyBindingDefOf.Misc6;
		hasDesignateAllFloatMenuOption = true;
		designateAllLabel = "UnforbidAllItems".Translate();
		tutorTag = "Unforbid";
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map) || c.Fogged(base.Map))
		{
			return false;
		}
		if (!c.GetThingList(base.Map).Any((Thing t) => CanDesignateThing(t).Accepted))
		{
			return "MessageMustDesignateUnforbiddable".Translate();
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (CanDesignateThing(thingList[i]).Accepted)
			{
				DesignateThing(thingList[i]);
			}
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t.def.category != ThingCategory.Item)
		{
			return false;
		}
		return t.TryGetComp<CompForbiddable>()?.Forbidden ?? false;
	}

	public override void DesignateThing(Thing t)
	{
		t.SetForbidden(value: false, warnOnFail: false);
	}
}
