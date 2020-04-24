using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsHarvest : Designator_Plants
	{
		public Designator_PlantsHarvest()
		{
			defaultLabel = "DesignatorHarvest".Translate();
			defaultDesc = "DesignatorHarvestDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Harvest;
			hotKey = KeyBindingDefOf.Misc2;
			designationDef = DesignationDefOf.HarvestPlant;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			AcceptanceReport result = base.CanDesignateThing(t);
			if (!result.Accepted)
			{
				return result;
			}
			Plant plant = (Plant)t;
			if (!plant.HarvestableNow || plant.def.plant.harvestTag != "Standard")
			{
				return "MessageMustDesignateHarvestable".Translate();
			}
			return true;
		}

		protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
		{
			return target.Thing.def.plant.harvestTag == "Standard";
		}
	}
}
