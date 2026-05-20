using Verse;

namespace RimWorld;

public class Alert_DormanyWakeUpDelay : Alert_ActionDelay
{
	private SignalAction_DormancyWakeUp wakeUpDelay;

	public Alert_DormanyWakeUpDelay()
	{
	}

	public Alert_DormanyWakeUpDelay(SignalAction_DormancyWakeUp wakeUpDelay)
	{
		this.wakeUpDelay = wakeUpDelay;
	}

	public override AlertReport GetReport()
	{
		if (wakeUpDelay == null)
		{
			return AlertReport.Inactive;
		}
		if (wakeUpDelay?.lord != null && !wakeUpDelay.lord.ownedPawns.NullOrEmpty())
		{
			return AlertReport.CulpritsAre(wakeUpDelay.lord.ownedPawns);
		}
		return AlertReport.Active;
	}

	public override string GetLabel()
	{
		return "AlertHostilesWakingUp".Translate(wakeUpDelay.lord.faction, wakeUpDelay.delayTicks.ToStringTicksToPeriodVerbose()).CapitalizeFirst();
	}

	public override TaggedString GetExplanation()
	{
		return "AlertHostilesWakingUpDesc".Translate(wakeUpDelay.lord.faction).CapitalizeFirst();
	}
}
