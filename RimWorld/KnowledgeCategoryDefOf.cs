namespace RimWorld;

[DefOf]
public static class KnowledgeCategoryDefOf
{
	[MayRequireAnomaly]
	public static KnowledgeCategoryDef Basic;

	[MayRequireAnomaly]
	public static KnowledgeCategoryDef Advanced;

	static KnowledgeCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(KnowledgeCategoryDefOf));
	}
}
