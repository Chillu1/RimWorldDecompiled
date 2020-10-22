using System.Linq;
using RimWorld;

namespace Verse
{
	public class HediffGiver_RandomAgeCurved : HediffGiver
	{
		public SimpleCurve ageFractionMtbDaysCurve;

		public int minPlayerPopulation;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			float x = (float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy;
			if (Rand.MTBEventOccurs(ageFractionMtbDaysCurve.Evaluate(x), 60000f, 60f) && (minPlayerPopulation <= 0 || pawn.Faction != Faction.OfPlayer || PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count() >= minPlayerPopulation) && TryApply(pawn))
			{
				SendLetter(pawn, cause);
			}
		}
	}
}
