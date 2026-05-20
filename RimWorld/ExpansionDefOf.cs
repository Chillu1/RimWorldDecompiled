namespace RimWorld;

[DefOf]
public static class ExpansionDefOf
{
	public static ExpansionDef Core;

	public static ExpansionDef Anomaly;

	static ExpansionDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ExpansionDefOf));
	}
}
