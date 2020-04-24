namespace RimWorld
{
	[DefOf]
	public static class BillRepeatModeDefOf
	{
		public static BillRepeatModeDef RepeatCount;

		public static BillRepeatModeDef TargetCount;

		public static BillRepeatModeDef Forever;

		static BillRepeatModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BillRepeatModeDefOf));
		}
	}
}
