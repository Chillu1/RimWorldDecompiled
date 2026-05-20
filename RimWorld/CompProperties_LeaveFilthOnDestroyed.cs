using Verse;

namespace RimWorld;

public class CompProperties_LeaveFilthOnDestroyed : CompProperties
{
	public int thickness;

	public ThingDef filthDef;

	public CompProperties_LeaveFilthOnDestroyed()
	{
		compClass = typeof(CompLeaveFilthOnDestroyed);
	}
}
