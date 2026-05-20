using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class CompProperties_Analyzable : CompProperties_Interactable
{
	public IntRange analysisRequiredRange = new IntRange(1, 1);

	public bool destroyedOnAnalyzed;

	public float analysisDurationHours = 1.5f;

	public bool showProgress;

	public bool allowRepeatAnalysis;

	[MustTranslate]
	public string progressedLetterLabel;

	[MustTranslate]
	public List<string> progressedLetters = new List<string>();

	public LetterDef progressedLetterDef;

	[MustTranslate]
	public string completedLetterLabel;

	[MustTranslate]
	public string completedLetter;

	public LetterDef completedLetterDef;

	[MustTranslate]
	public string repeatCompletedLetterLabel;

	[MustTranslate]
	public string repeatCompletedLetter;

	public LetterDef repeatCompletedLetterDef;

	public bool canStudyInPlace;

	public CompProperties_Analyzable()
	{
		compClass = typeof(CompAnalyzable);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (!string.IsNullOrEmpty(progressedLetterLabel) && !progressedLetters.NullOrEmpty() && progressedLetterDef == null)
		{
			yield return "Letter def is null (progressedLetterDef) for analyzable component";
		}
		if ((!string.IsNullOrEmpty(completedLetterLabel) || !string.IsNullOrEmpty(completedLetter)) && completedLetterDef == null)
		{
			yield return "Letter def is null (completedLetterDef) for analyzable component";
		}
		if ((!string.IsNullOrEmpty(repeatCompletedLetterLabel) || !string.IsNullOrEmpty(repeatCompletedLetter)) && repeatCompletedLetterDef == null)
		{
			yield return "Letter def is null (repeatCompletedLetterDef) for analyzable component";
		}
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
	}
}
