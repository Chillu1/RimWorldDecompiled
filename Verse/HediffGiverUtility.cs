using System.Collections.Generic;
using System.Linq;

namespace Verse;

public static class HediffGiverUtility
{
	public static bool TryApply(Pawn pawn, HediffDef hediff, IEnumerable<BodyPartDef> partsToAffect, bool canAffectAnyLivePart = false, int countToAffect = 1, List<Hediff> outAddedHediffs = null, bool useCoverage = true)
	{
		if (canAffectAnyLivePart || partsToAffect != null)
		{
			bool result = false;
			for (int i = 0; i < countToAffect; i++)
			{
				IEnumerable<BodyPartRecord> source = pawn.health.hediffSet.GetNotMissingParts();
				if (partsToAffect != null)
				{
					source = source.Where((BodyPartRecord p) => partsToAffect.Contains(p.def));
				}
				if (canAffectAnyLivePart)
				{
					source = source.Where((BodyPartRecord p) => p.def.alive);
				}
				source = source.Where((BodyPartRecord p) => !pawn.health.hediffSet.HasHediff(hediff, p) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)).ToList();
				if (!source.Any())
				{
					break;
				}
				Hediff hediff2 = HediffMaker.MakeHediff(partRecord: (!useCoverage) ? source.RandomElement() : source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs), def: hediff, pawn: pawn);
				pawn.health.AddHediff(hediff2);
				outAddedHediffs?.Add(hediff2);
				result = true;
			}
			return result;
		}
		if (!pawn.health.hediffSet.HasHediff(hediff))
		{
			Hediff hediff3 = HediffMaker.MakeHediff(hediff, pawn);
			pawn.health.AddHediff(hediff3);
			outAddedHediffs?.Add(hediff3);
			return true;
		}
		return false;
	}
}
