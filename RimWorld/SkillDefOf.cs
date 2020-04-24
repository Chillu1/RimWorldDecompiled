namespace RimWorld
{
	[DefOf]
	public static class SkillDefOf
	{
		public static SkillDef Construction;

		public static SkillDef Plants;

		public static SkillDef Intellectual;

		public static SkillDef Mining;

		public static SkillDef Shooting;

		public static SkillDef Melee;

		public static SkillDef Social;

		public static SkillDef Animals;

		public static SkillDef Cooking;

		public static SkillDef Medicine;

		public static SkillDef Artistic;

		public static SkillDef Crafting;

		static SkillDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SkillDefOf));
		}
	}
}
