using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualTargetFilter_AltarOrIdeogram : RitualTargetFilter_Altar
	{
		public RitualTargetFilter_AltarOrIdeogram()
		{
		}

		public RitualTargetFilter_AltarOrIdeogram(RitualTargetFilterDef def)
			: base(def)
		{
		}

		public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
		{
			TargetInfo targetInfo = BestTarget(initiator, selectedTarget);
			rejectionReason = "";
			if (!targetInfo.IsValid)
			{
				rejectionReason = "AbilityDisabledNoAltarIdeogramOrRitualsSpot".Translate();
				return false;
			}
			return true;
		}

		public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
		{
			yield return "RitualTargetGatherAltarIdeogramOrRitualSpotInfo".Translate();
		}

		protected override IEnumerable<Building> CandidateBuildings(Ideo ideo, Map map)
		{
			if (map == null)
			{
				yield break;
			}
			foreach (Precept_Building item in ideo.cachedPossibleBuildings.Where((Precept_Building b) => b.ThingDef.isAltar || b.ThingDef == ThingDefOf.Ideogram))
			{
				foreach (Building item2 in map.listerBuildings.AllBuildingsColonistOfDef(item.ThingDef))
				{
					yield return item2;
				}
			}
		}
	}
}
