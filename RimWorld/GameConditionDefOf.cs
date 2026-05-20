using Verse;

namespace RimWorld;

[DefOf]
public static class GameConditionDefOf
{
	public static GameConditionDef Eclipse;

	public static GameConditionDef PsychicDrone;

	public static GameConditionDef PsychicSoothe;

	public static GameConditionDef HeatWave;

	public static GameConditionDef ColdSnap;

	public static GameConditionDef Flashstorm;

	public static GameConditionDef VolcanicWinter;

	public static GameConditionDef ToxicFallout;

	public static GameConditionDef Aurora;

	[MayRequireRoyalty]
	public static GameConditionDef PsychicSuppression;

	[MayRequireRoyalty]
	public static GameConditionDef WeatherController;

	[MayRequireRoyalty]
	public static GameConditionDef EMIField;

	[MayRequireBiotech]
	public static GameConditionDef NoxiousHaze;

	[MayRequireAnomaly]
	public static GameConditionDef DeathPall;

	[MayRequireAnomaly]
	public static GameConditionDef GrayPall;

	[MayRequireAnomaly]
	public static GameConditionDef HateChantDrone;

	[MayRequireAnomaly]
	public static GameConditionDef UnnaturalHeat;

	[MayRequireAnomaly]
	public static GameConditionDef UnnaturalDarkness;

	[MayRequireAnomaly]
	public static GameConditionDef BloodRain;

	[MayRequireOdyssey]
	public static GameConditionDef Drought;

	[MayRequireOdyssey]
	public static GameConditionDef LavaFlow;

	[MayRequireOdyssey]
	public static GameConditionDef GillRot;

	static GameConditionDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(GameConditionDefOf));
	}
}
