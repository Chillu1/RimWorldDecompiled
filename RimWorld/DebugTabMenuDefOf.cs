namespace RimWorld;

[DefOf]
public static class DebugTabMenuDefOf
{
	public static DebugTabMenuDef Actions;

	public static DebugTabMenuDef Settings;

	public static DebugTabMenuDef Output;

	static DebugTabMenuDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(DebugTabMenuDefOf));
	}
}
