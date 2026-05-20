using Verse;

namespace RimWorld;

public class FocusStrengthOffset_Lit : FocusStrengthOffset
{
	public override string GetExplanation(Thing parent)
	{
		if (CanApply(parent))
		{
			return "StatsReport_Lit".Translate() + ": " + GetOffset(parent).ToStringWithSign("0%");
		}
		return GetExplanationAbstract();
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return "StatsReport_Lit".Translate() + ": " + offset.ToStringWithSign("0%");
	}

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		return offset;
	}

	public override bool CanApply(Thing parent, Pawn user = null)
	{
		return parent.TryGetComp<CompGlower>()?.Glows ?? false;
	}
}
