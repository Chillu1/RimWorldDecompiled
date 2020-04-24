namespace RimWorld
{
	[DefOf]
	public static class TransferableSorterDefOf
	{
		public static TransferableSorterDef Category;

		public static TransferableSorterDef MarketValue;

		static TransferableSorterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TransferableSorterDefOf));
		}
	}
}
