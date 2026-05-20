using Verse;

namespace RimWorld;

[DefOf]
public static class ThinkTreeDefOf
{
	[MayRequireAnomaly]
	public static ThinkTreeDef ShamblerConstant;

	static ThinkTreeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ThinkTreeDefOf));
	}
}
