using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_CampfireParty : RitualObligationTargetWorker_ThingDef
	{
		public RitualObligationTargetWorker_CampfireParty()
		{
		}

		public RitualObligationTargetWorker_CampfireParty(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			if (!ModLister.CheckIdeology("Campfire party"))
			{
				return false;
			}
			if (!base.CanUseTargetInternal(target, obligation).canUse)
			{
				return false;
			}
			Thing thing = target.Thing;
			CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
			if (compRefuelable != null && !compRefuelable.HasFuel)
			{
				return "RitualTargetCampfireNoFuel".Translate();
			}
			List<Thing> forCell = target.Map.listerBuldingOfDefInProximity.GetForCell(target.Cell, def.maxDrumDistance, ThingDefOf.Drum);
			bool flag = false;
			foreach (Thing item in forCell)
			{
				if (item.GetRoom() == thing.GetRoom())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return "RitualTargetNoDrum".Translate();
			}
			return true;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return "RitualTargetCampfirePartyInfo".Translate();
			yield return ThingDefOf.RitualSpot.label;
		}
	}
}
