using Verse;

namespace RimWorld;

[DefOf]
public static class MentalBreakDefOf
{
	public static MentalBreakDef Berserk;

	public static MentalBreakDef CorpseObsession;

	public static MentalBreakDef Catatonic;

	[MayRequireAnomaly]
	public static MentalBreakDef BerserkShort;

	[MayRequireAnomaly]
	public static MentalBreakDef CubeSculpting;

	[MayRequireAnomaly]
	public static MentalBreakDef HumanityBreak;

	static MentalBreakDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MentalBreakDefOf));
	}
}
