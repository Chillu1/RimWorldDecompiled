using Verse;

namespace RimWorld;

public class ReadingOutcomeDoerMentalBreak : BookOutcomeDoer
{
	public new BookOutcomeProperties_MentalBreak Props => (BookOutcomeProperties_MentalBreak)props;

	public override bool DoesProvidesOutcome(Pawn reader)
	{
		return false;
	}

	public override void OnBookGenerated(Pawn author = null)
	{
		float randomInRange = Props.chancePerHourRange.RandomInRange;
		randomInRange = Props.chanceMinMaxRange.ClampToRange(randomInRange);
		base.Book.SetMentalBreakChance(randomInRange);
	}
}
