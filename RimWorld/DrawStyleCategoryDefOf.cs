using Verse;

namespace RimWorld;

[DefOf]
public static class DrawStyleCategoryDefOf
{
	public static DrawStyleCategoryDef Paint;

	public static DrawStyleCategoryDef Plans;

	public static DrawStyleCategoryDef RemovePlans;

	public static DrawStyleCategoryDef RemoveZones;

	public static DrawStyleCategoryDef Mine;

	public static DrawStyleCategoryDef Areas;

	public static DrawStyleCategoryDef Zones;

	public static DrawStyleCategoryDef Orders;

	public static DrawStyleCategoryDef Cancel;

	public static DrawStyleCategoryDef Floors;

	public static DrawStyleCategoryDef Plants;

	public static DrawStyleCategoryDef FilledRectangle;

	static DrawStyleCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(DrawStyleCategoryDefOf));
	}
}
