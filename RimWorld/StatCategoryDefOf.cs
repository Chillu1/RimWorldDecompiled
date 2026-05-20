namespace RimWorld;

[DefOf]
public static class StatCategoryDefOf
{
	public static StatCategoryDef Basics;

	public static StatCategoryDef BasicsImportant;

	public static StatCategoryDef BasicsPawnImportant;

	public static StatCategoryDef BasicsNonPawnImportant;

	public static StatCategoryDef BasicsNonPawn;

	public static StatCategoryDef BasicsPawn;

	public static StatCategoryDef Animals;

	public static StatCategoryDef AnimalProductivity;

	public static StatCategoryDef Source;

	[MayRequireBiotech]
	public static StatCategoryDef Mechanoid;

	[MayRequireAnomaly]
	public static StatCategoryDef PsychicRituals;

	[MayRequireAnomaly]
	public static StatCategoryDef Containment;

	[MayRequireAnomaly]
	public static StatCategoryDef Serum;

	public static StatCategoryDef Apparel;

	public static StatCategoryDef Implant;

	public static StatCategoryDef Weapon;

	public static StatCategoryDef Weapon_Ranged;

	public static StatCategoryDef Weapon_Melee;

	public static StatCategoryDef Ability;

	public static StatCategoryDef Building;

	public static StatCategoryDef Terrain;

	public static StatCategoryDef PawnResistances;

	public static StatCategoryDef PawnPsyfocus;

	public static StatCategoryDef PawnHealth;

	public static StatCategoryDef PawnFood;

	public static StatCategoryDef PawnWork;

	public static StatCategoryDef PawnCombat;

	public static StatCategoryDef PawnSocial;

	public static StatCategoryDef PawnMisc;

	public static StatCategoryDef EquippedStatOffsets;

	public static StatCategoryDef StuffStatFactors;

	public static StatCategoryDef StuffStatOffsets;

	public static StatCategoryDef Surgery;

	public static StatCategoryDef CapacityEffects;

	public static StatCategoryDef Meditation;

	public static StatCategoryDef Drug;

	public static StatCategoryDef DrugAddiction;

	public static StatCategoryDef Genetics;

	static StatCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(StatCategoryDefOf));
	}
}
