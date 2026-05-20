namespace RimWorld;

[DefOf]
public static class EntityCodexEntryDefOf
{
	[MayRequireAnomaly]
	public static EntityCodexEntryDef UnnaturalCorpse;

	static EntityCodexEntryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(EntityCodexEntryDefOf));
	}
}
