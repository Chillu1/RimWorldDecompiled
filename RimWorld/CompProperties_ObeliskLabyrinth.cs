using Verse;

namespace RimWorld;

public class CompProperties_ObeliskLabyrinth : CompProperties_Interactable
{
	[MustTranslate]
	public string messageActivating;

	public CompProperties_ObeliskLabyrinth()
	{
		compClass = typeof(CompObelisk_Labyrinth);
	}
}
