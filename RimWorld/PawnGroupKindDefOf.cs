namespace RimWorld;

[DefOf]
public static class PawnGroupKindDefOf
{
	public static PawnGroupKindDef Combat;

	public static PawnGroupKindDef Trader;

	public static PawnGroupKindDef Peaceful;

	public static PawnGroupKindDef Settlement;

	public static PawnGroupKindDef Settlement_RangedOnly;

	[MayRequireIdeology]
	public static PawnGroupKindDef Miners;

	[MayRequireIdeology]
	public static PawnGroupKindDef Farmers;

	[MayRequireIdeology]
	public static PawnGroupKindDef Loggers;

	[MayRequireIdeology]
	public static PawnGroupKindDef Hunters;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Shamblers;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Fleshbeasts;

	[MayRequireAnomaly]
	public static PawnGroupKindDef FleshbeastsWithDreadmeld;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Sightstealers;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Metalhorrors;

	[MayRequireAnomaly]
	public static PawnGroupKindDef PsychicRitualSiege;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Gorehulks;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Devourers;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Noctols;

	[MayRequireAnomaly]
	public static PawnGroupKindDef SightstealersNoctols;

	[MayRequireAnomaly]
	public static PawnGroupKindDef Chimeras;

	static PawnGroupKindDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PawnGroupKindDefOf));
	}
}
