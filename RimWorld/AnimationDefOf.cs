using Verse;

namespace RimWorld;

[DefOf]
public static class AnimationDefOf
{
	[MayRequireAnomaly]
	public static AnimationDef ShamblerRise;

	[MayRequireAnomaly]
	public static AnimationDef RevenantSpasm;

	[MayRequireAnomaly]
	public static AnimationDef RevenantHypnotise;

	[MayRequireAnomaly]
	public static AnimationDef DevourerDigesting;

	[MayRequireAnomaly]
	public static AnimationDef UnnaturalCorpseAwokenKilling;

	[MayRequireAnomaly]
	public static AnimationDef DeathRefusalTwitches;

	[MayRequireAnomaly]
	public static AnimationDef HoldingPlatformLungeUp;

	[MayRequireAnomaly]
	public static AnimationDef HoldingPlatformLungeRight;

	[MayRequireAnomaly]
	public static AnimationDef HoldingPlatformLungeDown;

	[MayRequireAnomaly]
	public static AnimationDef HoldingPlatformLungeLeft;

	[MayRequireAnomaly]
	public static AnimationDef HoldingPlatformWiggleIntense;

	[MayRequireAnomaly]
	public static AnimationDef HoldingPlatformWiggleLight;

	static AnimationDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(AnimationDefOf));
	}
}
