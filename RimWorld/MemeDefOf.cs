namespace RimWorld;

[DefOf]
public static class MemeDefOf
{
	[MayRequireIdeology]
	public static MemeDef MaleSupremacy;

	[MayRequireIdeology]
	public static MemeDef FemaleSupremacy;

	[MayRequireIdeology]
	public static MemeDef TreeConnection;

	[MayRequireIdeology]
	public static MemeDef Tunneler;

	[MayRequireIdeology]
	public static MemeDef Darkness;

	static MemeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MemeDefOf));
	}
}
