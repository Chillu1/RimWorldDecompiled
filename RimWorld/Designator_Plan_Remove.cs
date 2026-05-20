using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Plan_Remove : Designator_Plan
{
	private readonly HashSet<Plan> justDesignated = new HashSet<Plan>();

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.RemovePlans;

	public Designator_Plan_Remove()
	{
		defaultLabel = "DesignatorPlanRemove".Translate();
		defaultDesc = "DesignatorPlanRemoveDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOff");
		soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
		soundDragChanged = null;
		soundSucceeded = SoundDefOf.Designate_PlanRemove;
		useMouseIcon = true;
		hotKey = KeyBindingDefOf.Misc8;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (base.Map.planManager.PlanAt(c) == null)
		{
			return false;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		Plan plan = base.Map.planManager.PlanAt(c);
		plan.RemoveCell(c);
		justDesignated.Add(plan);
	}

	protected override void FinalizeDesignationSucceeded()
	{
		base.FinalizeDesignationSucceeded();
		foreach (Plan item in justDesignated)
		{
			item.CheckContiguous();
		}
		justDesignated.Clear();
	}
}
