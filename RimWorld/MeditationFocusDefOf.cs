namespace RimWorld;

[DefOf]
public static class MeditationFocusDefOf
{
	[MayRequireRoyalty]
	public static MeditationFocusDef Natural;

	static MeditationFocusDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MeditationFocusDefOf));
	}
}
