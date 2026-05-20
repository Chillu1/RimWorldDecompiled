using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_InfestationDelay : Alert_ActionDelay
{
	private SignalAction_Infestation infestationAction;

	public Alert_InfestationDelay()
	{
	}

	public Alert_InfestationDelay(SignalAction_Infestation infestationAction)
	{
		this.infestationAction = infestationAction;
	}

	public override AlertReport GetReport()
	{
		if (infestationAction == null)
		{
			return AlertReport.Inactive;
		}
		if (infestationAction.overrideLoc.HasValue)
		{
			return AlertReport.CulpritIs(new GlobalTargetInfo(infestationAction.overrideLoc.Value, infestationAction.Map));
		}
		return AlertReport.Active;
	}

	public override string GetLabel()
	{
		return "AlertInfestationArriving".Translate(infestationAction.delayTicks.ToStringTicksToPeriodVerbose());
	}

	public override TaggedString GetExplanation()
	{
		return "AlertInfestationArrivingDesc".Translate();
	}
}
