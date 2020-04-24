using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Deconstruct : Designator
	{
		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Deconstruct;

		public Designator_Deconstruct()
		{
			defaultLabel = "DesignatorDeconstruct".Translate();
			defaultDesc = "DesignatorDeconstructDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Deconstruct;
			hotKey = KeyBindingDefOf.Designator_Deconstruct;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!DebugSettings.godMode && c.Fogged(base.Map))
			{
				return false;
			}
			if (TopDeconstructibleInCell(c, out AcceptanceReport reportToDisplay) == null)
			{
				return reportToDisplay;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			DesignateThing(TopDeconstructibleInCell(loc, out AcceptanceReport _));
		}

		private Thing TopDeconstructibleInCell(IntVec3 loc, out AcceptanceReport reportToDisplay)
		{
			reportToDisplay = AcceptanceReport.WasRejected;
			foreach (Thing item in from t in base.Map.thingGrid.ThingsAt(loc)
				orderby t.def.altitudeLayer descending
				select t)
			{
				AcceptanceReport acceptanceReport = CanDesignateThing(item);
				if (CanDesignateThing(item).Accepted)
				{
					reportToDisplay = AcceptanceReport.WasAccepted;
					return item;
				}
				if (!acceptanceReport.Reason.NullOrEmpty())
				{
					reportToDisplay = acceptanceReport;
				}
			}
			return null;
		}

		public override void DesignateThing(Thing t)
		{
			Thing innerIfMinified = t.GetInnerIfMinified();
			if (DebugSettings.godMode || innerIfMinified.GetStatValue(StatDefOf.WorkToBuild) == 0f || t.def.IsFrame)
			{
				t.Destroy(DestroyMode.Deconstruct);
			}
			else
			{
				base.Map.designationManager.AddDesignation(new Designation(t, Designation));
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Building building = t.GetInnerIfMinified() as Building;
			if (building == null)
			{
				return false;
			}
			if (building.def.category != ThingCategory.Building)
			{
				return false;
			}
			if (!building.DeconstructibleBy(Faction.OfPlayer))
			{
				if (building.Faction == Faction.OfMechanoids && building.def.building.IsDeconstructible)
				{
					return new AcceptanceReport("MessageMustDesignateDeconstructibleMechCluster".Translate());
				}
				return false;
			}
			if (base.Map.designationManager.DesignationOn(t, Designation) != null)
			{
				return false;
			}
			if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Uninstall) != null)
			{
				return false;
			}
			return true;
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
