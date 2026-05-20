using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Uninstall : Designator
{
	protected override DesignationDef Designation => DesignationDefOf.Uninstall;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Uninstall()
	{
		defaultLabel = "DesignatorUninstall".Translate();
		defaultDesc = "DesignatorUninstallDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Uninstall");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Deconstruct;
		hotKey = KeyBindingDefOf.Misc12;
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
		if (TopUninstallableInCell(c) == null)
		{
			return false;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		DesignateThing(TopUninstallableInCell(loc));
	}

	private Thing TopUninstallableInCell(IntVec3 loc)
	{
		foreach (Thing item in from t in base.Map.thingGrid.ThingsAt(loc)
			orderby t.def.altitudeLayer descending
			select t)
		{
			if (CanDesignateThing(item).Accepted)
			{
				return item;
			}
		}
		return null;
	}

	public override void DesignateThing(Thing t)
	{
		if (t.Faction != Faction.OfPlayer)
		{
			t.SetFaction(Faction.OfPlayer);
		}
		if (DebugSettings.godMode || t.GetStatValue(StatDefOf.WorkToBuild) == 0f || t.def.IsFrame)
		{
			t.Uninstall();
			return;
		}
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		if (ModsConfig.AnomalyActive && t is Building_HoldingPlatform { Occupied: not false } building_HoldingPlatform)
		{
			Messages.Message("MessageOccupiedHoldingPlatformUninstalled".Translate(), building_HoldingPlatform, MessageTypeDefOf.CautionInput);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!(t is Building building))
		{
			return false;
		}
		if (building.def.category != ThingCategory.Building)
		{
			return false;
		}
		if (!building.def.Minifiable)
		{
			return false;
		}
		if (!DebugSettings.godMode && building.Faction != Faction.OfPlayer && !building.def.building.alwaysUninstallable)
		{
			if (building.Faction != null)
			{
				return false;
			}
			if (!building.ClaimableBy(Faction.OfPlayer))
			{
				return false;
			}
		}
		if (base.Map.designationManager.DesignationOn(t, Designation) != null)
		{
			return false;
		}
		if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
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
