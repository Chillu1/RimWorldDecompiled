using Verse;

namespace RimWorld;

public class CompProperties_MechRelay : CompProperties
{
	[MustTranslate]
	public string activeInspectString;

	[MustTranslate]
	public string destabilizationInspectString;

	[MustTranslate]
	public string destabilizationMessage;

	[MustTranslate]
	public string crashedLetterLabel;

	[MustTranslate]
	public string crashedLetterText;

	public ThingDef crashedThingDef;

	public CompProperties_MechRelay()
	{
		compClass = typeof(CompMechRelay);
	}
}
