using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_AnyGatherSpotOrAltar : RitualObligationTargetWorker_AnyGatherSpot
{
	public RitualObligationTargetWorker_AnyGatherSpotOrAltar()
	{
	}

	public RitualObligationTargetWorker_AnyGatherSpotOrAltar(RitualObligationTargetFilterDef def)
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
		List<Thing> partySpot = map.listerThings.ThingsOfDef(ThingDefOf.PartySpot);
		for (int j = 0; j < partySpot.Count; j++)
		{
			yield return partySpot[j];
		}
		for (int j = 0; j < map.gatherSpotLister.activeSpots.Count; j++)
		{
			yield return map.gatherSpotLister.activeSpots[j].parent;
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		if (!target.HasThing)
		{
			return false;
		}
		Thing thing = target.Thing;
		if (def.colonistThingsOnly && (thing.Faction == null || !thing.Faction.IsPlayer))
		{
			return false;
		}
		if (thing.def == ThingDefOf.PartySpot)
		{
			return true;
		}
		if (thing.def == ThingDefOf.RitualSpot)
		{
			return true;
		}
		CompGatherSpot compGatherSpot = thing.TryGetComp<CompGatherSpot>();
		if (compGatherSpot != null && compGatherSpot.Active)
		{
			return true;
		}
		return RitualObligationTargetWorker_Altar.CanUseTargetWorker(target, obligation, parent.ideo);
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		yield return "RitualTargetGatherSpotInfo".Translate();
		foreach (string item in RitualObligationTargetWorker_Altar.GetTargetInfosWorker(parent.ideo))
		{
			yield return item;
		}
		yield return ThingDefOf.RitualSpot.LabelCap;
	}
}
