using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualTargetFilter_IdeoBuilding : RitualTargetFilter
{
	private List<Thing> candidateSpots = new List<Thing>();

	public RitualTargetFilter_IdeoBuilding()
	{
	}

	public RitualTargetFilter_IdeoBuilding(RitualTargetFilterDef def)
		: base(def)
	{
	}

	public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
	{
		TargetInfo targetInfo = BestTarget(initiator, selectedTarget);
		rejectionReason = "";
		if (!targetInfo.IsValid)
		{
			rejectionReason = "AbilitySpeechDisabledNoSpot".Translate();
			return false;
		}
		return true;
	}

	public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
	{
		Pawn pawn = (Pawn)initiator.Thing;
		Ideo ideo = pawn.Ideo;
		candidateSpots.Clear();
		for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
		{
			if (ideo.PreceptsListForReading[i] is Precept_Building precept_Building)
			{
				Thing thing = precept_Building.presenceDemand.BestBuilding(pawn.Map);
				if (thing != null && pawn.CanReach(thing, PathEndMode.Touch, pawn.NormalMaxDanger()))
				{
					candidateSpots.Add(thing);
				}
			}
		}
		candidateSpots.AddRange(ExtraCandidates(initiator));
		if (!candidateSpots.NullOrEmpty())
		{
			return candidateSpots.RandomElement();
		}
		if (!def.fallBackToGatherSpot)
		{
			return TargetInfo.Invalid;
		}
		candidateSpots.Clear();
		for (int j = 0; j < pawn.Map.gatherSpotLister.activeSpots.Count; j++)
		{
			ThingWithComps parent = pawn.Map.gatherSpotLister.activeSpots[j].parent;
			if (pawn.CanReach(parent, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				candidateSpots.Add(parent);
			}
		}
		if (!candidateSpots.NullOrEmpty())
		{
			return candidateSpots.OrderBy((Thing s) => s.Position.DistanceTo(pawn.Position)).First();
		}
		return TargetInfo.Invalid;
	}

	protected virtual IEnumerable<Thing> ExtraCandidates(TargetInfo initiator)
	{
		return Enumerable.Empty<Thing>();
	}

	public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
	{
		Ideo ideo = ((Pawn)initiator.Thing).Ideo;
		foreach (Precept_Building cachedPossibleBuilding in ideo.cachedPossibleBuildings)
		{
			yield return cachedPossibleBuilding.LabelCap;
		}
		yield return "RitualTargetGatherSpotInfo".Translate();
	}
}
