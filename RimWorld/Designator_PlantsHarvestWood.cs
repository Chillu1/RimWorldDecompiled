using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_PlantsHarvestWood : Designator_Plants
{
	private static readonly List<string> tmpIdeoMemberNames = new List<string>();

	private bool CanDesignateStumpsNow
	{
		get
		{
			if (Find.DesignatorManager.SelectedDesignator != this)
			{
				return true;
			}
			foreach (IntVec3 dragCell in Find.DesignatorManager.Dragger.DragCells)
			{
				Plant plant = dragCell.GetPlant(base.Map);
				if (plant != null && plant.def.plant.IsTree && !plant.def.plant.isStump && plant.HarvestableNow && base.Map.designationManager.DesignationOn(plant, designationDef) == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public Designator_PlantsHarvestWood()
	{
		defaultLabel = "DesignatorHarvestWood".Translate();
		defaultDesc = "DesignatorHarvestWoodDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_HarvestWood;
		hotKey = KeyBindingDefOf.Misc1;
		designationDef = DesignationDefOf.HarvestPlant;
		tutorTag = "PlantsHarvestWood";
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		AcceptanceReport result = base.CanDesignateThing(t);
		if (!result.Accepted)
		{
			return result;
		}
		Plant plant = (Plant)t;
		if (!plant.HarvestableNow || !plant.def.plant.IsTree)
		{
			return "MessageMustDesignateHarvestableWood".Translate();
		}
		if (t.TryGetComp(out CompPlantPreventCutting comp) && comp.PreventCutting)
		{
			return "MessageMustPlantCuttingForbidden".Translate();
		}
		if (t.def.plant.isStump && !CanDesignateStumpsNow)
		{
			return false;
		}
		return true;
	}

	protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
	{
		return target.Thing.def.plant.IsTree;
	}

	public override void DesignateThing(Thing t)
	{
		PossiblyWarnPlayerImportantPlantDesignateCut(t);
		if (ModsConfig.IdeologyActive && t.def.plant.IsTree && t.def.plant.treeLoversCareIfChopped)
		{
			PossiblyWarnPlayerOnDesignatingTreeCut();
		}
		base.DesignateThing(t);
	}

	public static void PossiblyWarnPlayerImportantPlantDesignateCut(Thing t)
	{
		if (t is Plant plant && plant.def.plant.warnIfMarkedForCut && (!plant.HarvestableNow || plant.def.plant.IsTree))
		{
			Messages.Message("MessageWarningCutImportantPlant".Translate(plant.LabelCap), plant, MessageTypeDefOf.CautionInput, historical: false);
		}
	}

	public static void PossiblyWarnPlayerOnDesignatingTreeCut()
	{
		tmpIdeoMemberNames.Clear();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			if (allIdeo.WarnPlayerOnDesignateChopTree)
			{
				tmpIdeoMemberNames.Add(Find.ActiveLanguageWorker.Pluralize(allIdeo.memberName));
			}
		}
		if (tmpIdeoMemberNames.Any())
		{
			Messages.Message("MessageWarningPlayerDesignatedTreeChopped".Translate(tmpIdeoMemberNames.ToCommaList(useAnd: true)), MessageTypeDefOf.CautionInput, historical: false);
		}
		tmpIdeoMemberNames.Clear();
	}
}
