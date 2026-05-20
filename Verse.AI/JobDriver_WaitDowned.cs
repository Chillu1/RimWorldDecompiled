using RimWorld;

namespace Verse.AI;

public class JobDriver_WaitDowned : JobDriver_Wait
{
	public override string GetReport()
	{
		if (pawn.Deathresting)
		{
			return ReportStringProcessed(SanguophageUtility.DeathrestJobReport(pawn));
		}
		if (!pawn.Awake())
		{
			return "DownedUnconscious".Translate();
		}
		if (!pawn.health.CanCrawl)
		{
			return "DownedCannotCrawl".Translate();
		}
		return base.GetReport();
	}

	public override void DecorateWaitToil(Toil wait)
	{
		base.DecorateWaitToil(wait);
		wait.AddFailCondition(() => !pawn.Downed);
	}
}
