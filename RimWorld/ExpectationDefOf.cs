namespace RimWorld
{
	[DefOf]
	public static class ExpectationDefOf
	{
		public static ExpectationDef ExtremelyLow;

		public static ExpectationDef VeryLow;

		public static ExpectationDef Low;

		public static ExpectationDef Moderate;

		public static ExpectationDef High;

		public static ExpectationDef SkyHigh;

		static ExpectationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ExpectationDefOf));
		}
	}
}
