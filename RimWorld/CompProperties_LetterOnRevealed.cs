using Verse;

namespace RimWorld;

public class CompProperties_LetterOnRevealed : CompProperties
{
	[MustTranslate]
	public string label;

	[MustTranslate]
	public string text;

	public LetterDef letterDef;

	public CompProperties_LetterOnRevealed()
	{
		compClass = typeof(CompLetterOnRevealed);
	}
}
