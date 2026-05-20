using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_MetalhorrorImplantation : IncidentWorker
{
	private static readonly SimpleCurve PathwayAgeWeightCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(0.7f, 0.3f)
	};

	private static readonly List<InfectionPathway> TmpPathways = new List<InfectionPathway>();

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!Find.Anomaly.CanNewMetalhorrorBiosignatureImplantOccur)
		{
			return false;
		}
		return GetPossiblePawns(map).Any();
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		GetAllPawnInfectionPathways(TmpPathways, map);
		if (TmpPathways.Empty())
		{
			return false;
		}
		InfectionPathway infectionPathway = TmpPathways.RandomElementByWeight((InfectionPathway pathway) => PathwayAgeWeightCurve.Evaluate(pathway.AgePercent));
		TmpPathways.Clear();
		MetalhorrorUtility.Infect(descResolved: infectionPathway.GetExplanation(HediffDefOf.MetalhorrorImplant), pawn: infectionPathway.OwnerPawn);
		return true;
	}

	private List<Pawn> GetPossiblePawns(Map map)
	{
		List<Pawn> freeColonistsAndPrisoners = map.mapPawns.FreeColonistsAndPrisoners;
		for (int num = freeColonistsAndPrisoners.Count - 1; num >= 0; num--)
		{
			Pawn pawn = freeColonistsAndPrisoners[num];
			if (!MetalhorrorUtility.CanBeInfected(pawn) || !pawn.infectionVectors.AnyPathwayForHediff(HediffDefOf.MetalhorrorImplant))
			{
				freeColonistsAndPrisoners.RemoveAt(num);
			}
		}
		return freeColonistsAndPrisoners;
	}

	private void GetAllPawnInfectionPathways(List<InfectionPathway> pathways, Map map)
	{
		pathways.Clear();
		List<Pawn> freeColonistsAndPrisoners = map.mapPawns.FreeColonistsAndPrisoners;
		for (int i = 0; i < freeColonistsAndPrisoners.Count; i++)
		{
			Pawn pawn = freeColonistsAndPrisoners[i];
			if (MetalhorrorUtility.CanBeInfected(pawn))
			{
				pathways.AddRange(pawn.infectionVectors.GetPathwaysForHediff(HediffDefOf.MetalhorrorImplant));
			}
		}
	}
}
