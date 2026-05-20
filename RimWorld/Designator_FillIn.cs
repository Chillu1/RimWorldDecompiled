using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_FillIn : Designator
{
	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	protected override DesignationDef Designation => DesignationDefOf.FillIn;

	public Designator_FillIn()
	{
		soundSucceeded = SoundDefOf.Tick_Low;
		defaultLabel = "DesignatorFillIn".Translate();
		defaultDesc = "DesignatorFillInDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/FillCrater");
		showReverseDesignatorDisabledReason = true;
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
		if (!c.GetThingList(base.Map).Any((Thing t) => CanDesignateThing(t).Accepted))
		{
			return false;
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
		return t is Crater && base.Map.designationManager.DesignationOn(t, Designation) == null;
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
	}
}
