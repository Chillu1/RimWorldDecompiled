namespace RimWorld
{
	[DefOf]
	public static class PawnTableDefOf
	{
		public static PawnTableDef Work;

		public static PawnTableDef Assign;

		public static PawnTableDef Restrict;

		public static PawnTableDef Animals;

		public static PawnTableDef Wildlife;

		static PawnTableDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnTableDefOf));
		}
	}
}
