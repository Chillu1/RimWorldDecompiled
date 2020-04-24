namespace RimWorld
{
	[DefOf]
	public static class PawnRelationDefOf
	{
		public static PawnRelationDef Kin;

		public static PawnRelationDef UncleOrAunt;

		public static PawnRelationDef Child;

		public static PawnRelationDef Grandchild;

		public static PawnRelationDef Grandparent;

		public static PawnRelationDef Sibling;

		public static PawnRelationDef HalfSibling;

		public static PawnRelationDef Cousin;

		public static PawnRelationDef NephewOrNiece;

		public static PawnRelationDef Spouse;

		public static PawnRelationDef Parent;

		public static PawnRelationDef GreatGrandchild;

		public static PawnRelationDef GreatGrandparent;

		public static PawnRelationDef GranduncleOrGrandaunt;

		public static PawnRelationDef Lover;

		public static PawnRelationDef ExLover;

		public static PawnRelationDef ExSpouse;

		public static PawnRelationDef Fiance;

		public static PawnRelationDef Bond;

		static PawnRelationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnRelationDefOf));
		}
	}
}
