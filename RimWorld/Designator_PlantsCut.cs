using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Designator_PlantsCut : Designator_Plants
{
	public static readonly Texture2D IconTex = ContentFinder<Texture2D>.Get("UI/Designators/CutPlants");

	public Designator_PlantsCut()
	{
		defaultLabel = "DesignatorCutPlants".Translate();
		defaultDesc = "DesignatorCutPlantsDesc".Translate();
		icon = IconTex;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_CutPlants;
		hotKey = KeyBindingDefOf.Misc3;
		designationDef = DesignationDefOf.CutPlant;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		AcceptanceReport result = base.CanDesignateThing(t);
		if (!result.Accepted)
		{
			return result;
		}
		if (t.TryGetComp(out CompPlantPreventCutting comp) && comp.PreventCutting)
		{
			return "MessageMustPlantCuttingForbidden".Translate();
		}
		return AffectsThing(t);
	}

	protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
	{
		return AffectsThing(target.Thing);
	}

	private bool AffectsThing(Thing t)
	{
		if (!(t is Plant plant))
		{
			return false;
		}
		if (!isOrder && plant.def.plant.IsTree)
		{
			return !plant.HarvestableNow;
		}
		return true;
	}

	public override void DesignateThing(Thing t)
	{
		Designator_PlantsHarvestWood.PossiblyWarnPlayerImportantPlantDesignateCut(t);
		if (ModsConfig.IdeologyActive && t.def.plant.IsTree && t.def.plant.treeLoversCareIfChopped)
		{
			Designator_PlantsHarvestWood.PossiblyWarnPlayerOnDesignatingTreeCut();
		}
		base.DesignateThing(t);
	}
}
