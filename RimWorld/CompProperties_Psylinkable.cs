using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Psylinkable : CompProperties
{
	public List<int> requiredSubplantCountPerPsylinkLevel;

	public MeditationFocusDef requiredFocus;

	public SoundDef linkSound;

	[MustTranslate]
	public string enoughPlantsLetterLabel;

	[MustTranslate]
	public string enoughPlantsLetterText;

	public CompProperties_Psylinkable()
	{
		compClass = typeof(CompPsylinkable);
	}
}
