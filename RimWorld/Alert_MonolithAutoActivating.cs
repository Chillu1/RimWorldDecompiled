using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_MonolithAutoActivating : Alert
{
	private List<Thing> targets = new List<Thing>(1);

	public Alert_MonolithAutoActivating()
	{
		defaultExplanation = "MonolithTwistingExplanation".Translate();
		requireAnomaly = true;
	}

	public override string GetLabel()
	{
		return "MonolithTwistingAlert".Translate() + ": " + Find.Anomaly.monolith.TicksUntilAutoActivate.ToStringTicksToPeriodVerbose();
	}

	public override AlertReport GetReport()
	{
		if (Find.Anomaly.monolith == null || !Find.Anomaly.monolith.IsAutoActivating)
		{
			return false;
		}
		targets.Clear();
		targets.Add(Find.Anomaly.monolith);
		return AlertReport.CulpritsAre(targets);
	}
}
