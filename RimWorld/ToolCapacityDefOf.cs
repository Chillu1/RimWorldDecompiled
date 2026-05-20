using Verse;

namespace RimWorld;

[DefOf]
public static class ToolCapacityDefOf
{
	public static ToolCapacityDef KickMaterialInEyes;

	static ToolCapacityDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ToolCapacityDefOf));
	}
}
