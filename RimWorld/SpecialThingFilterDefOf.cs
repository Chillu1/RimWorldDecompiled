using Verse;

namespace RimWorld;

[DefOf]
public static class SpecialThingFilterDefOf
{
	public static SpecialThingFilterDef AllowFresh;

	public static SpecialThingFilterDef AllowDeadmansApparel;

	public static SpecialThingFilterDef AllowNonDeadmansApparel;

	public static SpecialThingFilterDef AllowLargeCorpses;

	[MayRequireIdeology]
	public static SpecialThingFilterDef AllowVegetarian;

	[MayRequireIdeology]
	public static SpecialThingFilterDef AllowCarnivore;

	[MayRequireIdeology]
	public static SpecialThingFilterDef AllowCannibal;

	[MayRequireIdeology]
	public static SpecialThingFilterDef AllowInsectMeat;

	[MayRequireAnomaly]
	public static SpecialThingFilterDef AllowCorpsesUnnatural;

	static SpecialThingFilterDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(SpecialThingFilterDefOf));
	}
}
