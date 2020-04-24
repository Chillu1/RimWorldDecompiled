using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsCut : Designator_Plants
	{
		public Designator_PlantsCut()
		{
			defaultLabel = "DesignatorCutPlants".Translate();
			defaultDesc = "DesignatorCutPlantsDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/CutPlants");
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
			return AffectsThing(t);
		}

		protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
		{
			return AffectsThing(target.Thing);
		}

		private bool AffectsThing(Thing t)
		{
			Plant plant;
			if ((plant = (t as Plant)) == null)
			{
				return false;
			}
			if (!isOrder && plant.def.plant.IsTree)
			{
				return !plant.HarvestableNow;
			}
			return true;
		}
	}
}
