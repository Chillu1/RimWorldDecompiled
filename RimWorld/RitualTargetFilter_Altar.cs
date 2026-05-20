using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualTargetFilter_Altar : RitualTargetFilter
{
	public RitualTargetFilter_Altar()
	{
	}

	public RitualTargetFilter_Altar(RitualTargetFilterDef def)
		: base(def)
	{
	}

	public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
	{
		TargetInfo targetInfo = BestTarget(initiator, selectedTarget);
		rejectionReason = "";
		if (!targetInfo.IsValid)
		{
			rejectionReason = "AbilityDisabledNoAltarOrRitualsSpot".Translate();
			return false;
		}
		return true;
	}

	public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
	{
		if (!(initiator.Thing is Pawn pawn))
		{
			return null;
		}
		Thing thing = null;
		float num = 99999f;
		foreach (Building item in CandidateBuildings(pawn.Ideo, initiator.Thing.Map))
		{
			if (item.def.isAltar && pawn.CanReach(item, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				int lengthHorizontalSquared = (pawn.Position - item.Position).LengthHorizontalSquared;
				if ((float)lengthHorizontalSquared < num)
				{
					thing = item;
					num = lengthHorizontalSquared;
				}
			}
		}
		if (thing == null && def.fallbackToRitualSpot && pawn.Map != null)
		{
			foreach (Thing item2 in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot))
			{
				if (pawn.CanReach(item2, PathEndMode.Touch, pawn.NormalMaxDanger()))
				{
					int lengthHorizontalSquared2 = (pawn.Position - item2.Position).LengthHorizontalSquared;
					if ((float)lengthHorizontalSquared2 < num)
					{
						thing = item2;
						num = lengthHorizontalSquared2;
					}
				}
			}
		}
		return thing;
	}

	public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
	{
		yield return "RitualTargetGatherAltarOrRitualSpotInfo".Translate();
	}

	protected virtual IEnumerable<Building> CandidateBuildings(Ideo ideo, Map map)
	{
		if (map == null)
		{
			yield break;
		}
		foreach (Precept_Building item in ideo.cachedPossibleBuildings.Where((Precept_Building b) => b.ThingDef.isAltar))
		{
			foreach (Building item2 in map.listerBuildings.AllBuildingsColonistOfDef(item.ThingDef))
			{
				yield return item2;
			}
		}
	}
}
