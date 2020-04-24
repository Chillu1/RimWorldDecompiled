namespace RimWorld
{
	[DefOf]
	public static class BillStoreModeDefOf
	{
		public static BillStoreModeDef DropOnFloor;

		public static BillStoreModeDef BestStockpile;

		public static BillStoreModeDef SpecificStockpile;

		static BillStoreModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BillStoreModeDefOf));
		}
	}
}
