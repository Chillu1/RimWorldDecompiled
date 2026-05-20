namespace RimWorld;

[DefOf]
public static class ResearchTabDefOf
{
	public static ResearchTabDef Main;

	[MayRequireAnomaly]
	public static ResearchTabDef Anomaly;

	static ResearchTabDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ResearchTabDefOf));
	}
}
