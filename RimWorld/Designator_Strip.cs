using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Strip : Designator
{
	protected override DesignationDef Designation => DesignationDefOf.Strip;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Strip()
	{
		defaultLabel = "DesignatorStrip".Translate();
		defaultDesc = "DesignatorStripDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Strip");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Claim;
		hotKey = KeyBindingDefOf.Misc11;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (!StrippablesInCell(c).Any())
		{
			return "MessageMustDesignateStrippable".Translate();
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		foreach (Thing item in StrippablesInCell(c))
		{
			DesignateThing(item);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (base.Map.designationManager.DesignationOn(t, Designation) != null)
		{
			return false;
		}
		return StrippableUtility.CanBeStrippedByColony(t);
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(t);
	}

	private IEnumerable<Thing> StrippablesInCell(IntVec3 c)
	{
		if (c.Fogged(base.Map))
		{
			yield break;
		}
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (CanDesignateThing(thingList[i]).Accepted)
			{
				yield return thingList[i];
			}
		}
	}
}
