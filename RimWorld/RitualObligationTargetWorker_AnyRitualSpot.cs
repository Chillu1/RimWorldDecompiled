using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_AnyRitualSpot : RitualObligationTargetWorker_AnyGatherSpot
{
	public RitualObligationTargetWorker_AnyRitualSpot()
	{
	}

	public RitualObligationTargetWorker_AnyRitualSpot(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		if (ModLister.CheckIdeology("Ritual spot target"))
		{
			List<Thing> ritualSpots = map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot);
			for (int j = 0; j < ritualSpots.Count; j++)
			{
				yield return ritualSpots[j];
			}
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		if (!target.HasThing)
		{
			return false;
		}
		return target.Thing.def == ThingDefOf.RitualSpot;
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		yield return ThingDefOf.RitualSpot.label;
	}
}
