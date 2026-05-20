namespace RimWorld;

[DefOf]
public static class MonolithLevelDefOf
{
	[MayRequireAnomaly]
	public static MonolithLevelDef Inactive;

	[MayRequireAnomaly]
	public static MonolithLevelDef Waking;

	[MayRequireAnomaly]
	public static MonolithLevelDef VoidAwakened;

	[MayRequireAnomaly]
	public static MonolithLevelDef Gleaming;

	[MayRequireAnomaly]
	public static MonolithLevelDef Embraced;

	[MayRequireAnomaly]
	public static MonolithLevelDef Disrupted;

	static MonolithLevelDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MonolithLevelDefOf));
	}
}
