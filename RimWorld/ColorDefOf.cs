namespace RimWorld;

[DefOf]
public static class ColorDefOf
{
	public static ColorDef PlanGray;

	static ColorDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ColorDefOf));
	}
}
