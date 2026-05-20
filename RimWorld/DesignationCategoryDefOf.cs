using Verse;

namespace RimWorld;

[DefOf]
public static class DesignationCategoryDefOf
{
	public static DesignationCategoryDef Production;

	public static DesignationCategoryDef Floors;

	public static DesignationCategoryDef Zone;

	static DesignationCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(DesignationCategoryDefOf));
	}
}
