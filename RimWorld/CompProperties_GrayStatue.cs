using Verse;

namespace RimWorld;

public abstract class CompProperties_GrayStatue : CompProperties
{
	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterText;

	public LetterDef letterDef;

	public FloatRange triggerRadiusRange = new FloatRange(3f, 8f);
}
