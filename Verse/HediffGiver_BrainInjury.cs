using RimWorld;

namespace Verse
{
	public class HediffGiver_BrainInjury : HediffGiver
	{
		public float chancePerDamagePct;

		public string letterLabel;

		public string letter;

		public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
		{
			if (!(hediff is Hediff_Injury))
			{
				return false;
			}
			if (hediff.Part != pawn.health.hediffSet.GetBrain())
			{
				return false;
			}
			float num = hediff.Severity / hediff.Part.def.GetMaxHealth(pawn);
			if (Rand.Value < num * chancePerDamagePct && TryApply(pawn))
			{
				if ((pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony) && !letter.NullOrEmpty())
				{
					Find.LetterStack.ReceiveLetter(letterLabel, letter.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn), LetterDefOf.NegativeEvent, pawn);
				}
				return true;
			}
			return false;
		}
	}
}
