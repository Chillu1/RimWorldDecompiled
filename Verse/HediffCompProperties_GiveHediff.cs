namespace Verse;

public class HediffCompProperties_GiveHediff : HediffCompProperties
{
	public HediffDef hediffDef;

	public bool skipIfAlreadyExists;

	public float atSeverity = 1f;

	public bool disappearsAfterGiving;

	public LetterDef letterDef;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterText;

	public HediffCompProperties_GiveHediff()
	{
		compClass = typeof(HediffComp_GiveHediff);
	}
}
