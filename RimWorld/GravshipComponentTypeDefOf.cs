namespace RimWorld;

[DefOf]
public static class GravshipComponentTypeDefOf
{
	[MayRequireOdyssey]
	public static GravshipComponentTypeDef SignalJammer;

	static GravshipComponentTypeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(GravshipComponentTypeDefOf));
	}
}
