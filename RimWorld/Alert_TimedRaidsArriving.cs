using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_TimedRaidsArriving : Alert
{
	private List<TimedDetectionRaids> timedRaidsArrivingSoon = new List<TimedDetectionRaids>();

	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private void CalculateTargets()
	{
		timedRaidsArrivingSoon.Clear();
		targets.Clear();
		foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
		{
			foreach (WorldObjectComp allComp in allWorldObject.AllComps)
			{
				if (allComp is TimedDetectionRaids timedDetectionRaids && ShouldAlertTimedRaids(timedDetectionRaids))
				{
					timedRaidsArrivingSoon.Add(timedDetectionRaids);
					if (!targets.Contains(allWorldObject))
					{
						targets.Add(allWorldObject);
					}
				}
			}
		}
	}

	private bool ShouldAlertTimedRaids(TimedDetectionRaids timedDetectionRaids)
	{
		if (timedDetectionRaids.alertRaidsArrivingIn && timedDetectionRaids.DetectionCountdownStarted)
		{
			return timedDetectionRaids.RaidsSentCount == 0;
		}
		return false;
	}

	public override string GetLabel()
	{
		return "AlertTimedRaidsArrivingIn".Translate(timedRaidsArrivingSoon.MinBy((TimedDetectionRaids t) => t.TicksLeftToSendRaids).TicksLeftToSendRaids.ToStringTicksToPeriodVerbose());
	}

	public override TaggedString GetExplanation()
	{
		return "AlertTimedRaidsArrivingInDesc".Translate(timedRaidsArrivingSoon.Select((TimedDetectionRaids t) => "AlertTimedRaidsArrivingAt".Translate(t.parent.Label, t.TicksLeftToSendRaids.ToStringTicksToPeriodVerbose()).Resolve()).ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
