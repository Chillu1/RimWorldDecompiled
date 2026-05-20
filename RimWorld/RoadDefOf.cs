namespace RimWorld;

[DefOf]
public static class RoadDefOf
{
	public static RoadDef AncientAsphaltRoad;

	public static RoadDef AncientAsphaltHighway;

	static RoadDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(RoadDefOf));
	}
}
