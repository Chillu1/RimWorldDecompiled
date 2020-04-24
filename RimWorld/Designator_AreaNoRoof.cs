using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaNoRoof : Designator_Area
	{
		private static List<IntVec3> justAddedCells = new List<IntVec3>();

		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_AreaNoRoof()
		{
			defaultLabel = "DesignatorAreaNoRoofExpand".Translate();
			defaultDesc = "DesignatorAreaNoRoofExpandDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/NoRoofArea");
			hotKey = KeyBindingDefOf.Misc5;
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
			RoofDef roofDef = base.Map.roofGrid.RoofAt(c);
			if (roofDef != null && roofDef.isThickRoof)
			{
				return "MessageNothingCanRemoveThickRoofs".Translate();
			}
			return !base.Map.areaManager.NoRoof[c];
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			base.Map.areaManager.NoRoof[c] = true;
			justAddedCells.Add(c);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			for (int i = 0; i < justAddedCells.Count; i++)
			{
				base.Map.areaManager.BuildRoof[justAddedCells[i]] = false;
			}
			justAddedCells.Clear();
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}
	}
}
