using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_EjectFuel : Designator
{
	protected override DesignationDef Designation => DesignationDefOf.EjectFuel;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_EjectFuel()
	{
		defaultLabel = "DesignatorEjectFuel".Translate();
		defaultDesc = "DesignatorEjectFuelDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/EjectFuel");
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Claim;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (!RefuelablesInCell(c).Any())
		{
			return false;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		foreach (Thing item in RefuelablesInCell(c))
		{
			DesignateThing(item);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!t.TryGetComp(out CompRefuelable comp))
		{
			return false;
		}
		AcceptanceReport result = comp.CanEjectFuel();
		if (!result.Accepted)
		{
			return result;
		}
		return base.Map.designationManager.DesignationOn(t, Designation) == null;
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
	}

	private IEnumerable<Thing> RefuelablesInCell(IntVec3 c)
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
