using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Designator_Plan_CopySelection : Designator_Plan
{
	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public override bool DrawHighlight => false;

	public Designator_Plan_CopySelection()
	{
		useMouseIcon = true;
		hideMouseIcon = true;
		mouseText = "CommandCopyPlanSelectionMouse".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanCopySelection");
		defaultLabel = "CommandCopyPlanSelectionLabel".Translate();
		defaultDesc = "CommandCopyPlanSelectionDesc".Translate();
		hotKey = KeyBindingDefOf.Designator_Cancel;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 loc)
	{
		return loc.InBounds(base.Map);
	}

	public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
	{
		Designator_Plan_CopySelectionPaste designator_Plan_CopySelectionPaste = DesignatorUtility.FindAllowedDesignator<Designator_Plan_CopySelectionPaste>();
		designator_Plan_CopySelectionPaste.Initialize(cells);
		Find.DesignatorManager.Select(designator_Plan_CopySelectionPaste);
		SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera());
	}

	public override void Selected()
	{
		base.Selected();
		Find.Selector.SelectedPlan = null;
	}
}
