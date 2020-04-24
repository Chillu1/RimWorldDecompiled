namespace RimWorld
{
	[DefOf]
	public static class ThingSetMakerDefOf
	{
		public static ThingSetMakerDef MapGen_AncientTempleContents;

		public static ThingSetMakerDef MapGen_AncientPodContents;

		public static ThingSetMakerDef MapGen_DefaultStockpile;

		public static ThingSetMakerDef MapGen_PrisonCellStockpile;

		public static ThingSetMakerDef Reward_ItemsStandard;

		public static ThingSetMakerDef DebugCaravanInventory;

		public static ThingSetMakerDef DebugQuestDropPodsContents;

		public static ThingSetMakerDef TraderStock;

		public static ThingSetMakerDef ResourcePod;

		public static ThingSetMakerDef RefugeePod;

		public static ThingSetMakerDef Meteorite;

		public static ThingSetMakerDef VisitorGift;

		static ThingSetMakerDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingSetMakerDefOf));
		}
	}
}
