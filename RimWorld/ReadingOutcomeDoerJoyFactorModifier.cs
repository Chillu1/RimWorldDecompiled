using Verse;

namespace RimWorld;

public class ReadingOutcomeDoerJoyFactorModifier : BookOutcomeDoer
{
	public new BookOutcomeProperties_JoyFactorModifier Props => (BookOutcomeProperties_JoyFactorModifier)props;

	public override bool DoesProvidesOutcome(Pawn reader)
	{
		return true;
	}

	public override void OnBookGenerated(Pawn author = null)
	{
		base.Book.SetJoyFactor(BookUtility.GetNovelJoyFactorForQuality(base.Quality));
	}

	public override string GetBenefitsString(Pawn reader = null)
	{
		return string.Format(" - {0}: x{1}", "BookJoyFactor".Translate(), base.Book.JoyFactor.ToStringPercent());
	}
}
