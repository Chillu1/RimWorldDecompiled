using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_GravshipLaunch : RitualObligationTargetFilter
{
	public RitualObligationTargetWorker_GravshipLaunch()
	{
	}

	public RitualObligationTargetWorker_GravshipLaunch(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.PilotConsole);
		foreach (Thing item in list)
		{
			if (item.TryGetComp(out CompPilotConsole comp) && (bool)comp.CanUseNow())
			{
				yield return item;
			}
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		if (!target.HasThing)
		{
			return false;
		}
		if (!target.Thing.TryGetComp(out CompPilotConsole comp))
		{
			return false;
		}
		AcceptanceReport acceptanceReport = comp.CanUseNow();
		if (!acceptanceReport.Accepted)
		{
			return acceptanceReport.Reason;
		}
		return true;
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		yield return "RitualTargetPilotConsole".Translate();
	}
}
