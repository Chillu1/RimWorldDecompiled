using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_RemoveFoundation : Designator_RemoveFloor
{
	public Designator_RemoveFoundation()
	{
		defaultLabel = (ModsConfig.OdysseyActive ? "DesignatorRemoveFoundation".Translate() : "DesignatorRemoveBridge".Translate());
		defaultDesc = (ModsConfig.OdysseyActive ? "DesignatorRemoveFoundationDesc".Translate() : "DesignatorRemoveBridgeDesc".Translate());
		icon = ContentFinder<Texture2D>.Get("UI/Designators/RemoveBridge");
		soundSucceeded = SoundDefOf.Designate_RemoveBridge;
		hotKey = KeyBindingDefOf.Misc5;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map) || c.Fogged(base.Map))
		{
			return false;
		}
		if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFoundation) != null)
		{
			return false;
		}
		if (!base.Map.terrainGrid.CanRemoveFoundationAt(c))
		{
			return false;
		}
		Building edifice = c.GetEdifice(base.Map);
		if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
		{
			return false;
		}
		if (WorkGiver_ConstructRemoveFoundation.AnyBuildingBlockingFoundationRemoval(c, base.Map))
		{
			return "MessageCannotRemoveSupportingFoundation".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		if (DebugSettings.godMode)
		{
			base.Map.terrainGrid.RemoveFoundation(c, doLeavings: false);
		}
		else
		{
			base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemoveFoundation));
		}
	}
}
