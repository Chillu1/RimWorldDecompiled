using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Deconstruct : Designator
{
	protected override DesignationDef Designation => DesignationDefOf.Deconstruct;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Orders;

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
		showReverseDesignatorDisabledReason = true;
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
		if (TopDeconstructibleInCell(c, out var reportToDisplay) == null)
		{
			return reportToDisplay;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		DesignateThing(TopDeconstructibleInCell(loc, out var _));
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
			return;
		}
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		if (ModsConfig.AnomalyActive && t is Building_HoldingPlatform { Occupied: not false } building_HoldingPlatform)
		{
			Messages.Message("MessageOccupiedHoldingPlatformDeconstructed".Translate(), building_HoldingPlatform, MessageTypeDefOf.CautionInput);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!(t.GetInnerIfMinified() is Building building))
		{
			return false;
		}
		if (building.def.category != ThingCategory.Building)
		{
			return false;
		}
		AcceptanceReport acceptanceReport = building.DeconstructibleBy(Faction.OfPlayer);
		if (!acceptanceReport.Accepted)
		{
			if (building.def.IsNonDeconstructibleAttackableBuilding)
			{
				return "RemoveByAttackingTooltip".Translate();
			}
			return acceptanceReport.Reason;
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
