using Verse;

namespace RimWorld;

public class FocusStrengthOffset_Quality : FocusStrengthOffset_Curve
{
	public override bool NeedsToBeSpawned => false;

	protected override string ExplanationKey => "StatsReport_FromQuality";

	protected override float SourceValue(Thing parent)
	{
		parent.TryGetQuality(out var qc);
		return (int)qc;
	}

	public override float MaxOffset(Thing parent = null)
	{
		if (parent != null)
		{
			return 0f;
		}
		return base.MaxOffset((Thing)null);
	}

	public override float MinOffset(Thing parent = null)
	{
		if (parent != null)
		{
			return GetOffset(parent);
		}
		return curve[0].y;
	}
}
