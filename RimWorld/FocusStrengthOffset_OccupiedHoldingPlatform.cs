using Verse;

namespace RimWorld;

public class FocusStrengthOffset_OccupiedHoldingPlatform : FocusStrengthOffset
{
	public override string GetExplanation(Thing parent)
	{
		if (CanApply(parent))
		{
			return "StatsReport_Occupied".Translate() + ": " + GetOffset(parent).ToStringWithSign("0%");
		}
		return GetExplanationAbstract();
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return "StatsReport_Occupied".Translate() + ": " + offset.ToStringWithSign("0%");
	}

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		return offset;
	}

	public override bool CanApply(Thing parent, Pawn user = null)
	{
		if (!ModLister.CheckAnomaly("FocusStrengthOffset_OccupiedHoldingPlatform"))
		{
			return false;
		}
		if (parent is Building_HoldingPlatform building_HoldingPlatform)
		{
			return building_HoldingPlatform.Occupied;
		}
		return false;
	}
}
