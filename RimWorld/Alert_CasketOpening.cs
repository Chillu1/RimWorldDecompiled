using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_CasketOpening : Alert_ActionDelay
{
	private SignalAction_OpenCasket openCasketAction;

	private List<Thing> culprints = new List<Thing>();

	public Alert_CasketOpening()
	{
	}

	public Alert_CasketOpening(SignalAction_OpenCasket openCasketAction)
	{
		this.openCasketAction = openCasketAction;
		culprints.AddRange(openCasketAction.caskets);
	}

	public override AlertReport GetReport()
	{
		if (openCasketAction == null)
		{
			return AlertReport.Inactive;
		}
		return AlertReport.CulpritsAre(culprints);
	}

	public override string GetLabel()
	{
		return "AlertCasketOpening".Translate(openCasketAction.delayTicks.ToStringTicksToPeriodVerbose());
	}

	public override TaggedString GetExplanation()
	{
		return "AlertCasketOpeningDesc".Translate();
	}
}
