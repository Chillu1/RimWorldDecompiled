using Verse;

namespace RimWorld;

public class FocusStrengthOffset_RoomImpressiveness : FocusStrengthOffset_Curve
{
	protected override string ExplanationKey => "StatsReport_RoomImpressiveness";

	protected override float SourceValue(Thing parent)
	{
		return parent.GetRoom()?.GetStat(RoomStatDefOf.Impressiveness) ?? 0f;
	}
}
