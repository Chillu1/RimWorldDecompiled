using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaIgnoreRoof : Designator_Area
	{
		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_AreaIgnoreRoof()
		{
			defaultLabel = "DesignatorAreaIgnoreRoofExpand".Translate();
			defaultDesc = "DesignatorAreaIgnoreRoofExpandDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/IgnoreRoofArea");
			hotKey = KeyBindingDefOf.Misc11;
			soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneAdd;
			useMouseIcon = true;
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
			return base.Map.areaManager.BuildRoof[c] || base.Map.areaManager.NoRoof[c];
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			base.Map.areaManager.BuildRoof[c] = false;
			base.Map.areaManager.NoRoof[c] = false;
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}
	}
}
