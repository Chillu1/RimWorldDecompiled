namespace RimWorld;

[DefOf]
public static class FactionDefOf
{
	public static FactionDef PlayerColony;

	public static FactionDef PlayerTribe;

	public static FactionDef Ancients;

	public static FactionDef AncientsHostile;

	public static FactionDef Mechanoid;

	public static FactionDef Insect;

	public static FactionDef Pirate;

	public static FactionDef OutlanderCivil;

	public static FactionDef TribeCivil;

	public static FactionDef OutlanderRough;

	public static FactionDef TribeRough;

	[MayRequireRoyalty]
	public static FactionDef Empire;

	[MayRequireRoyalty]
	public static FactionDef OutlanderRefugee;

	[MayRequireIdeology]
	public static FactionDef Beggars;

	[MayRequireIdeology]
	public static FactionDef Pilgrims;

	[MayRequireBiotech]
	public static FactionDef PirateWaster;

	[MayRequireBiotech]
	public static FactionDef Sanguophages;

	[MayRequireAnomaly]
	public static FactionDef HoraxCult;

	[MayRequireAnomaly]
	public static FactionDef Entities;

	[MayRequireOdyssey]
	public static FactionDef TradersGuild;

	[MayRequireOdyssey]
	public static FactionDef Salvagers;

	static FactionDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
	}
}
