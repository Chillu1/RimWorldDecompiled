using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsHarvestWood : Designator_Plants
	{
		public Designator_PlantsHarvestWood()
		{
			defaultLabel = "DesignatorHarvestWood".Translate();
			defaultDesc = "DesignatorHarvestWoodDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Harvest;
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
			return true;
		}

		protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
		{
			return target.Thing.def.plant.IsTree;
		}

		public override void DesignateThing(Thing t)
		{
			if (t.def == ThingDefOf.Plant_TreeAnima)
			{
				Messages.Message("MessageWarningCutAnimaTree".Translate(), t, MessageTypeDefOf.CautionInput, historical: false);
			}
			base.DesignateThing(t);
		}
	}
}
