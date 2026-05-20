using Verse;

namespace RimWorld;

[DefOf]
public static class PathGridDefOf
{
	public static PathGridDef Normal;

	public static PathGridDef FenceBlocked;

	public static PathGridDef Flying;

	static PathGridDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PathGridDefOf));
	}
}
