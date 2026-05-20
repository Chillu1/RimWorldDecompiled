namespace RimWorld;

[DefOf]
public static class DifficultyDefOf
{
	public static DifficultyDef Easy;

	public static DifficultyDef Rough;

	static DifficultyDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(DifficultyDefOf));
	}
}
