using System;
using Verse;

namespace RimWorld;

public class LandingOutcomeDef : Def
{
	public float weight;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterText;

	public LetterDef letterDef;

	public Type workerClass;

	private LandingOutcomeWorker worker;

	public LandingOutcomeWorker Worker => worker ?? (worker = (LandingOutcomeWorker)Activator.CreateInstance(workerClass, this));
}
