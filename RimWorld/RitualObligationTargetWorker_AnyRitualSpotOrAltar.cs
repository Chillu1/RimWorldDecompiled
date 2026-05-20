using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_AnyRitualSpotOrAltar : RitualObligationTargetWorker_AnyGatherSpot
{
	public RitualObligationTargetWorker_AnyRitualSpotOrAltar()
	{
	}

	public RitualObligationTargetWorker_AnyRitualSpotOrAltar(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		if (!ModLister.CheckIdeology("Altar target"))
		{
			yield break;
		}
		foreach (TargetInfo item in RitualObligationTargetWorker_Altar.GetTargetsWorker(obligation, map, parent.ideo))
		{
			yield return item;
		}
		List<Thing> ritualSpots = map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot);
		for (int j = 0; j < ritualSpots.Count; j++)
		{
			yield return ritualSpots[j];
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		if (!target.HasThing)
		{
			return false;
		}
		if (target.Thing.def == ThingDefOf.RitualSpot)
		{
			return true;
		}
		return RitualObligationTargetWorker_Altar.CanUseTargetWorker(target, obligation, parent.ideo);
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		foreach (string item in RitualObligationTargetWorker_Altar.GetTargetInfosWorker(parent.ideo))
		{
			yield return item;
		}
		yield return ThingDefOf.RitualSpot.label;
	}
}
