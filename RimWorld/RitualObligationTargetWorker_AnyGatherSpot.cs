using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_AnyGatherSpot : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_AnyGatherSpot()
		{
		}

		public RitualObligationTargetWorker_AnyGatherSpot(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
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
			CompGatherSpot compGatherSpot = thing.TryGetComp<CompGatherSpot>();
			if (compGatherSpot != null && compGatherSpot.Active)
			{
				return true;
			}
			return false;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return "RitualTargetGatherSpotInfo".Translate();
		}
	}
}
