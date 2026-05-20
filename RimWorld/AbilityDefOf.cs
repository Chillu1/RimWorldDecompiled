namespace RimWorld;

[DefOf]
public static class AbilityDefOf
{
	[MayRequireRoyalty]
	public static AbilityDef Speech;

	[MayRequireBiotech]
	public static AbilityDef ReimplantXenogerm;

	[MayRequireBiotech]
	public static AbilityDef ResurrectionMech;

	[MayRequireAnomaly]
	public static AbilityDef EntitySkip;

	[MayRequireAnomaly]
	public static AbilityDef UnnaturalCorpseSkip;

	[MayRequireAnomaly]
	public static AbilityDef ConsumeLeap_Devourer;

	[MayRequireOdyssey]
	public static AbilityDef SludgeSpew;

	[MayRequireOdyssey]
	public static AbilityDef EggSpew;

	static AbilityDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
	}
}
