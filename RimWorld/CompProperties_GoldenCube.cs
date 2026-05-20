using Verse;

namespace RimWorld;

public class CompProperties_GoldenCube : CompProperties_Interactable
{
	[MustTranslate]
	public string letterDeactivatedLabel;

	[MustTranslate]
	public string letterDeactivatedDesc;

	[MustTranslate]
	public string letterDeactivatedAppend;

	public CompProperties_GoldenCube()
	{
		compClass = typeof(CompGoldenCube);
	}
}
