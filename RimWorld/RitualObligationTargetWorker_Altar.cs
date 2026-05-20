using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_Altar : RitualObligationTargetFilter
{
	public RitualObligationTargetWorker_Altar()
	{
	}

	public RitualObligationTargetWorker_Altar(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		if (!ModLister.CheckIdeology("Altar target"))
		{
			yield break;
		}
		Ideo ideo = parent.ideo;
		foreach (TargetInfo item in GetTargetsWorker(obligation, map, ideo))
		{
			yield return item;
		}
	}

	public static IEnumerable<TargetInfo> GetTargetsWorker(RitualObligation obligation, Map map, Ideo ideo)
	{
		for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
		{
			if (!(ideo.PreceptsListForReading[i] is Precept_Building precept_Building) || !precept_Building.ThingDef.isAltar)
			{
				continue;
			}
			foreach (Thing item in precept_Building.presenceDemand.AllBuildings(map))
			{
				yield return item;
			}
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		return CanUseTargetWorker(target, obligation, parent.ideo);
	}

	public static bool CanUseTargetWorker(TargetInfo target, RitualObligation obligation, Ideo ideo)
	{
		if (!(target.Thing is Building { Faction: not null } building) || !building.Faction.IsPlayer)
		{
			return false;
		}
		if (!GetTargetsWorker(obligation, building.Map, ideo).Contains(building))
		{
			return false;
		}
		return true;
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		foreach (string item in GetTargetInfosWorker(parent.ideo))
		{
			yield return item;
		}
	}

	public static IEnumerable<string> GetTargetInfosWorker(Ideo ideo)
	{
		for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
		{
			if (ideo.PreceptsListForReading[i] is Precept_Building precept_Building && precept_Building.ThingDef.isAltar)
			{
				yield return precept_Building.LabelCap;
			}
		}
	}

	public override List<string> MissingTargetBuilding(Ideo ideo)
	{
		if (!GetTargetInfos(null).Any())
		{
			return new List<string> { "Altar".Translate() };
		}
		return null;
	}
}
