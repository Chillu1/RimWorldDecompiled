using Verse;

namespace RimWorld;

[DefOf]
public static class ThingCategoryDefOf
{
	public static ThingCategoryDef Foods;

	public static ThingCategoryDef Manufactured;

	public static ThingCategoryDef Root;

	public static ThingCategoryDef Apparel;

	public static ThingCategoryDef ApparelArmor;

	public static ThingCategoryDef ApparelUtility;

	public static ThingCategoryDef PlantFoodRaw;

	public static ThingCategoryDef PlantMatter;

	public static ThingCategoryDef ResourcesRaw;

	public static ThingCategoryDef Items;

	public static ThingCategoryDef Neurotrainers;

	public static ThingCategoryDef NeurotrainersPsycast;

	public static ThingCategoryDef NeurotrainersSkill;

	public static ThingCategoryDef Techprints;

	public static ThingCategoryDef BuildingsArt;

	public static ThingCategoryDef Weapons;

	public static ThingCategoryDef Medicine;

	public static ThingCategoryDef Drugs;

	public static ThingCategoryDef BodyParts;

	public static ThingCategoryDef Chunks;

	public static ThingCategoryDef StoneChunks;

	public static ThingCategoryDef StoneBlocks;

	public static ThingCategoryDef MeatRaw;

	public static ThingCategoryDef Leathers;

	public static ThingCategoryDef Textiles;

	public static ThingCategoryDef Wools;

	public static ThingCategoryDef Buildings;

	public static ThingCategoryDef BuildingsSpecial;

	public static ThingCategoryDef Corpses;

	public static ThingCategoryDef CorpsesHumanlike;

	public static ThingCategoryDef CorpsesAnimal;

	public static ThingCategoryDef CorpsesMechanoid;

	public static ThingCategoryDef EggsUnfertilized;

	public static ThingCategoryDef EggsFertilized;

	public static ThingCategoryDef Animals;

	public static ThingCategoryDef ArmorHeadgear;

	public static ThingCategoryDef Stumps;

	public static ThingCategoryDef BookEffects;

	[MayRequireOdyssey]
	public static ThingCategoryDef Fish;

	[MayRequireOdyssey]
	public static ThingCategoryDef WeaponsUnique;

	static ThingCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOf));
	}
}
