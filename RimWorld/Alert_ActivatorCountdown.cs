using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_ActivatorCountdown : Alert
{
	private List<Thing> activatorCountdownsResult = new List<Thing>();

	private List<Thing> ActivatorCountdowns
	{
		get
		{
			activatorCountdownsResult.Clear();
			foreach (Map map in Find.Maps)
			{
				if (!map.mapPawns.AnyColonistSpawned)
				{
					continue;
				}
				foreach (Thing item in map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.ActivatorCountdown)))
				{
					CompSendSignalOnCountdown compSendSignalOnCountdown = item.TryGetComp<CompSendSignalOnCountdown>();
					if (compSendSignalOnCountdown != null && compSendSignalOnCountdown.ticksLeft > 0)
					{
						activatorCountdownsResult.Add(item);
					}
				}
			}
			return activatorCountdownsResult;
		}
	}

	public Alert_ActivatorCountdown()
	{
		defaultPriority = AlertPriority.High;
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(ActivatorCountdowns);
	}

	public override string GetLabel()
	{
		int count = activatorCountdownsResult.Count;
		if (count > 1)
		{
			return "ActivatorCountdownMultiple".Translate(count);
		}
		if (count == 0)
		{
			return string.Empty;
		}
		CompSendSignalOnCountdown compSendSignalOnCountdown = activatorCountdownsResult[0].TryGetComp<CompSendSignalOnCountdown>();
		return "ActivatorCountdown".Translate(compSendSignalOnCountdown.ticksLeft.ToStringTicksToPeriodVerbose());
	}

	public override TaggedString GetExplanation()
	{
		int count = activatorCountdownsResult.Count;
		if (count > 1)
		{
			return "ActivatorCountdownDescMultiple".Translate(count);
		}
		if (count == 0)
		{
			return string.Empty;
		}
		return "ActivatorCountdownDesc".Translate();
	}
}
