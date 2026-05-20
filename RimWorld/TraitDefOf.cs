namespace RimWorld;

[DefOf]
public static class TraitDefOf
{
	public static TraitDef Nudist;

	public static TraitDef Brawler;

	public static TraitDef Abrasive;

	public static TraitDef DrugDesire;

	public static TraitDef Ascetic;

	public static TraitDef Psychopath;

	public static TraitDef Greedy;

	public static TraitDef Kind;

	public static TraitDef Gay;

	public static TraitDef Bisexual;

	public static TraitDef Asexual;

	public static TraitDef Industriousness;

	public static TraitDef DislikesMen;

	public static TraitDef DislikesWomen;

	public static TraitDef AnnoyingVoice;

	public static TraitDef CreepyBreathing;

	public static TraitDef Bloodlust;

	public static TraitDef Pyromaniac;

	public static TraitDef Transhumanist;

	public static TraitDef BodyPurist;

	public static TraitDef Undergrounder;

	public static TraitDef GreatMemory;

	public static TraitDef Jealous;

	public static TraitDef Wimp;

	[MayRequireAnomaly]
	public static TraitDef PerfectMemory;

	[MayRequireAnomaly]
	public static TraitDef Occultist;

	[MayRequireAnomaly]
	public static TraitDef Joyous;

	[MayRequireAnomaly]
	public static TraitDef Disturbing;

	[MayRequireAnomaly]
	public static TraitDef VoidFascination;

	static TraitDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
	}
}
