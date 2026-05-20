using Verse;

namespace RimWorld;

public class NegativeFishingOutcomeDef : Def
{
	public ThingDef fishType;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterText;

	public LetterDef letterDef;

	public DamageDef damageDef;

	public IntRange damageAmountRange;

	public HediffDef addsHediff;

	public float hediffSeverity;
}
