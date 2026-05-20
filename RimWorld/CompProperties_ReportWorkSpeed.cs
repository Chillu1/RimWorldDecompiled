using Verse;

namespace RimWorld;

public class CompProperties_ReportWorkSpeed : CompProperties
{
	public StatDef workSpeedStat;

	public CompProperties_ReportWorkSpeed()
	{
		compClass = typeof(CompReportWorkSpeed);
	}
}
