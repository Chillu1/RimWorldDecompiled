namespace RimWorld;

[DefOf]
public static class RenderSkipFlagDefOf
{
	public static RenderSkipFlagDef None;

	public static RenderSkipFlagDef Hair;

	public static RenderSkipFlagDef Head;

	public static RenderSkipFlagDef Beard;

	public static RenderSkipFlagDef Eyes;

	[MayRequireIdeology]
	public static RenderSkipFlagDef Tattoos;

	static RenderSkipFlagDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(RenderSkipFlagDefOf));
	}
}
