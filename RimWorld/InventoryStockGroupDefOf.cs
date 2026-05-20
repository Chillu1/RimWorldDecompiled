using Verse;

namespace RimWorld
{
	[DefOf]
	public static class InventoryStockGroupDefOf
	{
		public static InventoryStockGroupDef Medicine;

		static InventoryStockGroupDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InventoryStockGroupDefOf));
		}
	}
}
