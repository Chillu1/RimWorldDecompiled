namespace RimWorld;

[DefOf]
public static class ExpectationDefOf
{
	public static ExpectationDef ExtremelyLow;

	public static ExpectationDef VeryLow;

	public static ExpectationDef Low;

	public static ExpectationDef Moderate;

	public static ExpectationDef High;

	static ExpectationDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ExpectationDefOf));
	}
}
