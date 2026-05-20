using Verse;

namespace RimWorld;

[DefOf]
public static class GenStepDefOf
{
	public static GenStepDef PreciousLump;

	[MayRequireAnomaly]
	public static GenStepDef HarbingerTrees;

	[MayRequireOdyssey]
	public static GenStepDef GravshipMarker;

	[MayRequireOdyssey]
	public static GenStepDef ReserveGravshipArea;

	[MayRequireOdyssey]
	public static GenStepDef AncientRuins_Special;

	[MayRequireOdyssey]
	public static GenStepDef AncientStockpile;

	static GenStepDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(GenStepDefOf));
	}
}
