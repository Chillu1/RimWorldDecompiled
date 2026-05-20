using Verse;

namespace RimWorld;

public class Alert_FuelNodeIgnition : Alert_ActionDelay
{
	private SignalAction_StartWick startWickAction;

	public Alert_FuelNodeIgnition()
	{
	}

	public Alert_FuelNodeIgnition(SignalAction_StartWick startWickAction)
	{
		this.startWickAction = startWickAction;
	}

	public override AlertReport GetReport()
	{
		if (startWickAction == null)
		{
			return AlertReport.Inactive;
		}
		return AlertReport.CulpritIs(startWickAction.thingWithWick);
	}

	public override string GetLabel()
	{
		return "AlertFuelNodeIgniting".Translate(startWickAction.delayTicks.ToStringTicksToPeriodVerbose());
	}

	public override TaggedString GetExplanation()
	{
		return "AlertFuelNodeIgnitingDesc".Translate();
	}
}
