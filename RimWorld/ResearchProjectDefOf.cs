using Verse;

namespace RimWorld;

[DefOf]
public static class ResearchProjectDefOf
{
	public static ResearchProjectDef CarpetMaking;

	public static ResearchProjectDef MicroelectronicsBasics;

	public static ResearchProjectDef Batteries;

	public static ResearchProjectDef ColoredLights;

	[MayRequireBiotech]
	public static ResearchProjectDef BasicMechtech;

	[MayRequireBiotech]
	public static ResearchProjectDef Archogenetics;

	[MayRequireAnomaly]
	public static ResearchProjectDef BasicPsychicRituals;

	[MayRequireAnomaly]
	public static ResearchProjectDef AdvancedPsychicRituals;

	[MayRequireAnomaly]
	public static ResearchProjectDef BioferriteExtraction;

	[MayRequireOdyssey]
	public static ResearchProjectDef Fishing;

	[MayRequireOdyssey]
	public static ResearchProjectDef BasicGravtech;

	static ResearchProjectDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectDefOf));
	}
}
