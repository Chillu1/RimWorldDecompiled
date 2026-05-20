namespace RimWorld;

[DefOf]
public static class MutantDefOf
{
	[MayRequireAnomaly]
	public static MutantDef Shambler;

	[MayRequireAnomaly]
	public static MutantDef Ghoul;

	[MayRequireAnomaly]
	public static MutantDef AwokenCorpse;

	static MutantDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MutantDefOf));
	}
}
