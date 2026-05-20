namespace RimWorld;

[DefOf]
public static class AnomalyPlaystyleDefOf
{
	[MayRequireAnomaly]
	public static AnomalyPlaystyleDef Standard;

	[MayRequireAnomaly]
	public static AnomalyPlaystyleDef AmbientHorror;

	static AnomalyPlaystyleDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(AnomalyPlaystyleDefOf));
	}
}
