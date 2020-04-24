using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_RemoveFloor : Designator
	{
		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_RemoveFloor()
		{
			defaultLabel = "DesignatorRemoveFloor".Translate();
			defaultDesc = "DesignatorRemoveFloorDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/RemoveFloor");
			useMouseIcon = true;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			soundSucceeded = SoundDefOf.Designate_SmoothSurface;
			hotKey = KeyBindingDefOf.Misc1;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map) || c.Fogged(base.Map))
			{
				return false;
			}
			if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor) != null)
			{
				return false;
			}
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
			{
				return false;
			}
			if (!base.Map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				return "TerrainMustBeRemovable".Translate();
			}
			if (WorkGiver_ConstructRemoveFloor.AnyBuildingBlockingFloorRemoval(c, base.Map))
			{
				return false;
			}
			return AcceptanceReport.WasAccepted;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (DebugSettings.godMode)
			{
				base.Map.terrainGrid.RemoveTopLayer(c);
			}
			else
			{
				base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemoveFloor));
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
