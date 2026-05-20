using Verse;

namespace RimWorld;

public class CompProperties_GrowsFleshmassLump : CompProperties
{
	public IntRange maxFleshmassRange;

	public IntRange ticksBetweenFleshmassRange;

	public CompProperties_GrowsFleshmassLump()
	{
		compClass = typeof(CompGrowsFleshmassLump);
	}
}
