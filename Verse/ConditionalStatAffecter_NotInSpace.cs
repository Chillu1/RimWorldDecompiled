using RimWorld;

namespace Verse;

public class ConditionalStatAffecter_NotInSpace : ConditionalStatAffecter_InSpace
{
	public override string Label => "StatsReport_NotInSpace".Translate();

	public override bool Applies(StatRequest req)
	{
		return !base.Applies(req);
	}
}
