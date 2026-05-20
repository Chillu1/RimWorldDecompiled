namespace RimWorld;

[DefOf]
public static class PrefabDefOf
{
	[MayRequireOdyssey]
	public static PrefabDef AncientUplink;

	[MayRequireOdyssey]
	public static PrefabDef SurveyScanner;

	[MayRequireOdyssey]
	public static PrefabDef CerebrexCore;

	[MayRequireOdyssey]
	public static PrefabDef CorridorBarricade;

	static PrefabDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PrefabDefOf));
	}
}
