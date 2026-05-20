using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_IdeoBuildingOrRitualSpot : RitualObligationTargetFilter
{
	public RitualObligationTargetWorker_IdeoBuildingOrRitualSpot()
	{
	}

	public RitualObligationTargetWorker_IdeoBuildingOrRitualSpot(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		return Enumerable.Empty<TargetInfo>();
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		if (!ModLister.CheckIdeology("Ideo building target"))
		{
			return false;
		}
		if (!(target.Thing is Building { Faction: not null } building) || !building.Faction.IsPlayer)
		{
			return false;
		}
		if (building.def == ThingDefOf.RitualSpot)
		{
			return true;
		}
		for (int i = 0; i < parent.ideo.PreceptsListForReading.Count; i++)
		{
			if (parent.ideo.PreceptsListForReading[i] is Precept_Building precept_Building && building.def == precept_Building.ThingDef)
			{
				return true;
			}
		}
		if (building.TryGetComp<CompGatherSpot>() != null)
		{
			return true;
		}
		return false;
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		for (int i = 0; i < parent.ideo.PreceptsListForReading.Count; i++)
		{
			if (parent.ideo.PreceptsListForReading[i] is Precept_Building precept_Building)
			{
				yield return precept_Building.LabelCap;
			}
		}
		yield return ThingDefOf.RitualSpot.label;
		yield return "RitualTargetGatherSpotInfo".Translate();
	}

	public override List<string> MissingTargetBuilding(Ideo ideo)
	{
		return null;
	}
}
