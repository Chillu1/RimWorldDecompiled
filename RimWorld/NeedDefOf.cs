namespace RimWorld
{
	[DefOf]
	public static class NeedDefOf
	{
		public static NeedDef Food;

		public static NeedDef Rest;

		public static NeedDef Joy;

		static NeedDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(NeedDefOf));
		}
	}
}
