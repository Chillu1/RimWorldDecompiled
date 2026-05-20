using Verse;

namespace RimWorld;

public class FocusStrengthOffset_ThroneSatisfiesRequirements : FocusStrengthOffset
{
	public override bool DependsOnPawn => true;

	public override string GetExplanation(Thing parent)
	{
		return GetExplanationAbstract();
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return "StatsReport_SatisfiesTitle".Translate() + ": " + offset.ToStringWithSign("0%");
	}

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		return offset;
	}

	public override bool CanApply(Thing parent, Pawn user = null)
	{
		if (user == null)
		{
			return false;
		}
		return user.royalty?.AnyUnmetThroneroomRequirements() == false;
	}
}
