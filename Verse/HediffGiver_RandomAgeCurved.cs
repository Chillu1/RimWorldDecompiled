using System.Linq;
using RimWorld;

namespace Verse;

public class HediffGiver_RandomAgeCurved : HediffGiver
{
	public SimpleCurve ageFractionMtbDaysCurve;

	public int minPlayerPopulation;

	public override float ChanceFactor(Pawn pawn)
	{
		if (pawn.IsMutant && pawn.mutant.Def.disableAging)
		{
			return 0f;
		}
		return base.ChanceFactor(pawn);
	}

	public override void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
		float x = (float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy;
		if (Rand.MTBEventOccurs(ageFractionMtbDaysCurve.Evaluate(x), 60000f, 60f) && (minPlayerPopulation <= 0 || pawn.Faction != Faction.OfPlayer || PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended.Count() >= minPlayerPopulation) && TryApply(pawn))
		{
			SendLetter(pawn, cause);
		}
	}
}
