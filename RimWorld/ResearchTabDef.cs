using Verse;

namespace RimWorld;

public class ResearchTabDef : Def
{
	[MustTranslate]
	public string generalTitle = "";

	[MustTranslate]
	public string generalDescription = "";

	public bool visibleByDefault = true;

	public int minMonolithLevelVisible = -1;

	public string tutorTag;
}
