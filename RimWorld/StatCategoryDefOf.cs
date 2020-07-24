namespace RimWorld
{
	[DefOf]
	public static class StatCategoryDefOf
	{
		public static StatCategoryDef Basics;

		public static StatCategoryDef BasicsImportant;

		public static StatCategoryDef BasicsPawnImportant;

		public static StatCategoryDef BasicsNonPawnImportant;

		public static StatCategoryDef BasicsNonPawn;

		public static StatCategoryDef BasicsPawn;

		public static StatCategoryDef Apparel;

		public static StatCategoryDef Implant;

		public static StatCategoryDef Weapon;

		public static StatCategoryDef Ability;

		public static StatCategoryDef Building;

		public static StatCategoryDef PawnWork;

		public static StatCategoryDef PawnCombat;

		public static StatCategoryDef PawnSocial;

		public static StatCategoryDef PawnMisc;

		public static StatCategoryDef EquippedStatOffsets;

		public static StatCategoryDef StuffStatFactors;

		public static StatCategoryDef StuffStatOffsets;

		public static StatCategoryDef StuffOfEquipmentStatFactors;

		public static StatCategoryDef Surgery;

		public static StatCategoryDef CapacityEffects;

		public static StatCategoryDef Meditation;

		public static StatCategoryDef Drug;

		public static StatCategoryDef DrugAddiction;

		static StatCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StatCategoryDefOf));
		}
	}
}
