using Verse;

namespace RimWorld;

[DefOf]
public static class SubcameraDefOf
{
	public static SubcameraDef WaterDepth;

	[MayRequireOdyssey]
	public static SubcameraDef GravshipMask;

	static SubcameraDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(SubcameraDefOf));
	}
}
