using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaBuildRoof : Designator_Area
	{
		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_AreaBuildRoof()
		{
			defaultLabel = "DesignatorAreaBuildRoofExpand".Translate();
			defaultDesc = "DesignatorAreaBuildRoofExpandDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/BuildRoofArea");
			hotKey = KeyBindingDefOf.Misc9;
			soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneAdd;
			useMouseIcon = true;
			tutorTag = "AreaBuildRoofExpand";
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
			return !base.Map.areaManager.BuildRoof[c];
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			base.Map.areaManager.BuildRoof[c] = true;
			base.Map.areaManager.NoRoof[c] = false;
		}

		public override bool ShowWarningForCell(IntVec3 c)
		{
			foreach (Thing item in base.Map.thingGrid.ThingsAt(c))
			{
				if (item.def.plant != null && item.def.plant.interferesWithRoof)
				{
					Messages.Message("MessageRoofIncompatibleWithPlant".Translate(item), MessageTypeDefOf.CautionInput, historical: false);
					return true;
				}
			}
			return false;
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}
	}
}
