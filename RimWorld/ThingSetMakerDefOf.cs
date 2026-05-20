namespace RimWorld;

[DefOf]
public static class ThingSetMakerDefOf
{
	public static ThingSetMakerDef MapGen_AncientTempleContents;

	public static ThingSetMakerDef MapGen_AncientPodContents;

	public static ThingSetMakerDef MapGen_DefaultStockpile;

	public static ThingSetMakerDef MapGen_PrisonCellStockpile;

	public static ThingSetMakerDef MapGen_AncientComplexRoomLoot_Default;

	[MayRequireIdeology]
	public static ThingSetMakerDef MapGen_AncientComplexRoomLoot_Better;

	[MayRequireIdeology]
	public static ThingSetMakerDef MapGen_AncientComplex_SecurityCrate;

	[MayRequireAnomaly]
	public static ThingSetMakerDef MapGen_FleshSackLoot;

	[MayRequireOdyssey]
	public static ThingSetMakerDef MapGen_ScarlandsAncientPodContents;

	[MayRequireOdyssey]
	public static ThingSetMakerDef MapGen_ScarlandsHermeticCrate;

	[MayRequireOdyssey]
	public static ThingSetMakerDef MapGen_HighValueCrate;

	[MayRequireOdyssey]
	public static ThingSetMakerDef MapGen_AbandonedColonyStockpile;

	public static ThingSetMakerDef Reward_ItemsStandard;

	[MayRequireAnomaly]
	public static ThingSetMakerDef Reward_GrayBox;

	[MayRequireAnomaly]
	public static ThingSetMakerDef Reward_GrayBoxLowReward;

	public static ThingSetMakerDef DebugCaravanInventory;

	public static ThingSetMakerDef DebugQuestDropPodsContents;

	public static ThingSetMakerDef TraderStock;

	public static ThingSetMakerDef ResourcePod;

	public static ThingSetMakerDef RefugeePod;

	public static ThingSetMakerDef Meteorite;

	public static ThingSetMakerDef VisitorGift;

	[MayRequireIdeology]
	public static ThingSetMakerDef Reward_ReliquaryPilgrims;

	[MayRequireOdyssey]
	public static ThingSetMakerDef Reward_UniqueWeapon;

	[MayRequireOdyssey]
	public static ThingSetMakerDef Reward_ArcheanSeed;

	static ThingSetMakerDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ThingSetMakerDefOf));
	}
}
